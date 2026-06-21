using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmileDesk.Data;
using SmileDesk.Models;
using SmileDesk.Services;
using SmileDesk.ViewModels;

namespace SmileDesk.Controllers
{
    [Authorize(Roles = "Donor")]
    public class DonorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RazorpayService _razorpay;
        private readonly IWebHostEnvironment _env;

        public DonorController(ApplicationDbContext db, UserManager<ApplicationUser> um,
            RazorpayService rp, IWebHostEnvironment env)
        {
            _db = db; _userManager = um; _razorpay = rp; _env = env;
        }

        private async Task<DonorProfile?> GetProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _db.DonorProfiles.FirstOrDefaultAsync(d => d.UserId == user!.Id);
        }

        // ─── Dashboard ────────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var vm = new DonorDashboardViewModel
            {
                Profile = profile,
                MyItems = await _db.DonationItems
                    .Where(d => d.DonorProfileId == profile.Id && !d.IsDeleted)
                    .OrderByDescending(d => d.PostedOn).Take(5).ToListAsync(),
                MyDonations = await _db.MoneyDonations
                    .Include(m => m.NGOProfile)
                    .Where(m => m.DonorProfileId == profile.Id)
                    .OrderByDescending(m => m.DonatedOn).Take(5).ToListAsync(),
                PendingRequests = await _db.ItemRequests
                    .Include(r => r.NGOProfile)
                    .Include(r => r.DonationItem)
                    .Where(r => r.DonationItem!.DonorProfileId == profile.Id
                             && r.Status == ItemRequestStatus.Pending)
                    .ToListAsync(),
                RecentEvents = await _db.NGOEvents
                    .Include(e => e.NGOProfile)
                    .Where(e => e.IsActive && !e.IsDeleted)
                    .OrderByDescending(e => e.PostedOn).Take(4).ToListAsync(),
                TotalItemsDonated = await _db.DonationItems
                    .CountAsync(d => d.DonorProfileId == profile.Id
                                  && d.Status == DonationItemStatus.Donated),
                TotalMoneyDonated = await _db.MoneyDonations
                    .Where(m => m.DonorProfileId == profile.Id
                              && m.Status == PaymentStatus.Success)
                    .SumAsync(m => (decimal?)m.Amount) ?? 0
            };
            return View(vm);
        }

        // ─── Profile (Create / Edit) ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var existing = await GetProfileAsync();
            if (existing != null) return RedirectToAction("EditProfile");
            return View(new DonorProfileViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(DonorProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            var profile = new DonorProfile
            {
                UserId = user!.Id,
                DonorName = vm.DonorName,
                ContributionType = vm.ContributionType,
                DateOfBirth = vm.DateOfBirth,
                MobileNumber = vm.MobileNumber,
                DoorNo = vm.DoorNo,
                StreetName = vm.StreetName,
                City = vm.City,
                State = vm.State,
                Country = vm.Country
            };
            _db.DonorProfiles.Add(profile);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile created. Welcome to Smile Desk.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            return View(new DonorProfileViewModel
            {
                DonorName = profile.DonorName,
                ContributionType = profile.ContributionType,
                DateOfBirth = profile.DateOfBirth,
                MobileNumber = profile.MobileNumber,
                DoorNo = profile.DoorNo,
                StreetName = profile.StreetName,
                City = profile.City,
                State = profile.State,
                Country = profile.Country
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(DonorProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            profile.DonorName = vm.DonorName;
            profile.ContributionType = vm.ContributionType;
            profile.DateOfBirth = vm.DateOfBirth;
            profile.MobileNumber = vm.MobileNumber;
            profile.DoorNo = vm.DoorNo;
            profile.StreetName = vm.StreetName;
            profile.City = vm.City;
            profile.State = vm.State;
            profile.Country = vm.Country;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // ─── Donation Items — Create ────────────────────────────────────────
        [HttpGet] public IActionResult PostItem() => View(new DonationItemViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PostItem(DonationItemViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            string? imagePath = await SaveImageAsync(vm.Image, "items");

            _db.DonationItems.Add(new DonationItem
            {
                DonorProfileId = profile.Id,
                Title = vm.Title,
                Category = vm.Category,
                Description = vm.Description,
                Condition = vm.Condition,
                ImagePath = imagePath
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Item posted. NGOs can now request it.";
            return RedirectToAction("MyItems");
        }

        // ─── Donation Items — Edit ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var item = await _db.DonationItems
                .FirstOrDefaultAsync(d => d.Id == id && d.DonorProfileId == profile.Id && !d.IsDeleted);
            if (item == null) return NotFound();

            if (item.Status != DonationItemStatus.Available)
            {
                TempData["Error"] = "This item already has activity on it and can no longer be edited.";
                return RedirectToAction("MyItems");
            }

            return View(new DonationItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Category = item.Category,
                Description = item.Description,
                Condition = item.Condition
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(DonationItemViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var item = await _db.DonationItems
                .FirstOrDefaultAsync(d => d.Id == vm.Id && d.DonorProfileId == profile.Id && !d.IsDeleted);
            if (item == null) return NotFound();

            item.Title = vm.Title;
            item.Category = vm.Category;
            item.Description = vm.Description;
            item.Condition = vm.Condition;
            item.UpdatedOn = DateTime.Now;

            if (vm.Image != null && vm.Image.Length > 0)
                item.ImagePath = await SaveImageAsync(vm.Image, "items") ?? item.ImagePath;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Item updated successfully.";
            return RedirectToAction("MyItems");
        }

        // ─── Donation Items — Delete (soft) ──────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var item = await _db.DonationItems
                .FirstOrDefaultAsync(d => d.Id == id && d.DonorProfileId == profile.Id);
            if (item == null) return NotFound();

            if (item.Status == DonationItemStatus.Accepted || item.Status == DonationItemStatus.PickedUp)
            {
                TempData["Error"] = "This item has an accepted request in progress and cannot be removed.";
                return RedirectToAction("MyItems");
            }

            item.IsDeleted = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Item removed from listings.";
            return RedirectToAction("MyItems");
        }

        // ─── My Items ─────────────────────────────────────────────────────────
        public async Task<IActionResult> MyItems()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var items = await _db.DonationItems
                .Where(d => d.DonorProfileId == profile.Id && !d.IsDeleted)
                .Include(d => d.Requests).ThenInclude(r => r.NGOProfile)
                .OrderByDescending(d => d.PostedOn)
                .ToListAsync();
            return View(items);
        }

        // ─── Accept / Reject Item Request ─────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RespondToRequest(int requestId, bool accept)
        {
            var request = await _db.ItemRequests
                .Include(r => r.DonationItem)
                .Include(r => r.NGOProfile)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null) return NotFound();

            request.Status = accept ? ItemRequestStatus.Accepted : ItemRequestStatus.Rejected;
            request.RespondedOn = DateTime.Now;

            if (accept && request.DonationItem != null)
                request.DonationItem.Status = DonationItemStatus.Accepted;
            else if (request.DonationItem != null)
                request.DonationItem.Status = DonationItemStatus.Available;

            await _db.SaveChangesAsync();
            TempData["Success"] = accept
                ? $"Request accepted. {request.NGOProfile?.OrganizationName} will contact you to arrange pickup."
                : "Request declined.";
            return RedirectToAction("MyItems");
        }

        // ─── Confirm physical handover to NGO ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmHandover(int requestId)
        {
            var request = await _db.ItemRequests
                .Include(r => r.DonationItem)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null || request.Status != ItemRequestStatus.Accepted) return NotFound();

            request.Status = ItemRequestStatus.PickedUp;
            request.PickedUpOn = DateTime.Now;
            if (request.DonationItem != null)
                request.DonationItem.Status = DonationItemStatus.Donated;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Handover confirmed. Thank you for your contribution!";
            return RedirectToAction("MyItems");
        }

        // ─── Money Donation - Initiate ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Donate(int ngoId, int? eventId)
        {
            var ngo = await _db.NGOProfiles.FindAsync(ngoId);
            if (ngo == null || !ngo.IsApproved) return NotFound();

            var vm = new MoneyDonationViewModel
            {
                NGOProfileId = ngoId,
                NGOEventId = eventId,
                NGOName = ngo.OrganizationName
            };

            if (eventId.HasValue)
            {
                var ev = await _db.NGOEvents.FindAsync(eventId.Value);
                vm.EventTitle = ev?.Title;
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InitiatePayment(MoneyDonationViewModel vm)
        {
            if (!ModelState.IsValid) return View("Donate", vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var ngo = await _db.NGOProfiles.FindAsync(vm.NGOProfileId);
            if (ngo == null) return NotFound();

            var donation = new MoneyDonation
            {
                DonorProfileId = profile.Id,
                NGOProfileId = vm.NGOProfileId,
                NGOEventId = vm.NGOEventId,
                Amount = vm.Amount,
                Message = vm.Message,
                Status = PaymentStatus.Pending
            };
            _db.MoneyDonations.Add(donation);
            await _db.SaveChangesAsync();

            // Split directly to the NGO's linked Razorpay account when available
            // (Razorpay Route). Otherwise the order is created normally and the
            // platform settles with the NGO off-platform until Route is approved.
            bool canRoute = !string.IsNullOrEmpty(ngo.RazorpayAccountId);
            string orderId = await _razorpay.CreateOrderAsync(
                vm.Amount, "INR", $"SD-{donation.Id}", canRoute, ngo.RazorpayAccountId);

            donation.RazorpayOrderId = orderId;
            donation.RoutedToNgo = canRoute;
            await _db.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            ViewBag.RazorpayKeyId = _razorpay.GetKeyId();
            ViewBag.OrderId = orderId;
            ViewBag.Amount = (int)(vm.Amount * 100);
            ViewBag.DonationId = donation.Id;
            ViewBag.DonorName = profile.DonorName;
            ViewBag.DonorEmail = user!.Email;
            ViewBag.DonorPhone = profile.MobileNumber;
            ViewBag.NGOName = ngo.OrganizationName;

            return View("PaymentPage");
        }

        [HttpPost]
        public async Task<IActionResult> PaymentCallback(PaymentCallbackViewModel vm)
        {
            var donation = await _db.MoneyDonations
                .Include(d => d.NGOEvent)
                .FirstOrDefaultAsync(d => d.Id == vm.DonationId);

            if (donation == null) return NotFound();

            bool valid = _razorpay.VerifySignature(
                vm.RazorpayOrderId, vm.RazorpayPaymentId, vm.RazorpaySignature);

            if (valid)
            {
                donation.Status = PaymentStatus.Success;
                donation.RazorpayPaymentId = vm.RazorpayPaymentId;
                donation.RazorpaySignature = vm.RazorpaySignature;

                if (donation.NGOEventId.HasValue && donation.NGOEvent != null)
                    donation.NGOEvent.FundsRaised += donation.Amount;

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Payment of \u20b9{donation.Amount:N0} successful! Thank you for your generosity.";
                return RedirectToAction("PaymentSuccess", new { id = donation.Id });
            }
            else
            {
                donation.Status = PaymentStatus.Failed;
                await _db.SaveChangesAsync();
                TempData["Error"] = "Payment verification failed. Please contact support.";
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> PaymentSuccess(int id)
        {
            var donation = await _db.MoneyDonations
                .Include(d => d.NGOProfile)
                .Include(d => d.NGOEvent)
                .FirstOrDefaultAsync(d => d.Id == id);
            return View(donation);
        }

        public async Task<IActionResult> MyDonations()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var donations = await _db.MoneyDonations
                .Include(m => m.NGOProfile)
                .Include(m => m.NGOEvent)
                .Where(m => m.DonorProfileId == profile.Id)
                .OrderByDescending(m => m.DonatedOn)
                .ToListAsync();
            return View(donations);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private async Task<string?> SaveImageAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subfolder}/{fileName}";
        }
    }
}
