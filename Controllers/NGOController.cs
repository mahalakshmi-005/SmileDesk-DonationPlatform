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
    [Authorize(Roles = "NGO")]
    public class NGOController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly RazorpayService _razorpay;

        public NGOController(ApplicationDbContext db, UserManager<ApplicationUser> um,
            IWebHostEnvironment env, RazorpayService razorpay)
        {
            _db = db; _userManager = um; _env = env; _razorpay = razorpay;
        }

        private async Task<NGOProfile?> GetProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _db.NGOProfiles.FirstOrDefaultAsync(n => n.UserId == user!.Id);
        }

        // ─── Dashboard ────────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var vm = new NGODashboardViewModel
            {
                Profile = profile,
                MyEvents = await _db.NGOEvents
                    .Where(e => e.NGOProfileId == profile.Id && !e.IsDeleted)
                    .OrderByDescending(e => e.PostedOn).Take(5).ToListAsync(),
                MyItemRequests = await _db.ItemRequests
                    .Include(r => r.DonationItem).ThenInclude(d => d!.DonorProfile)
                    .Where(r => r.NGOProfileId == profile.Id)
                    .OrderByDescending(r => r.RequestedOn).Take(5).ToListAsync(),
                ReceivedDonations = await _db.MoneyDonations
                    .Include(m => m.DonorProfile)
                    .Where(m => m.NGOProfileId == profile.Id && m.Status == PaymentStatus.Success)
                    .OrderByDescending(m => m.DonatedOn).Take(5).ToListAsync(),
                TotalFundsReceived = await _db.MoneyDonations
                    .Where(m => m.NGOProfileId == profile.Id && m.Status == PaymentStatus.Success)
                    .SumAsync(m => (decimal?)m.Amount) ?? 0,
                ItemsReceived = await _db.ItemRequests
                    .CountAsync(r => r.NGOProfileId == profile.Id && r.Status == ItemRequestStatus.PickedUp),
                BankDetailsNeeded = profile.IsApproved && !profile.BankDetailsSubmitted
            };
            return View(vm);
        }

        // ─── Profile (Create / Edit) ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var existing = await GetProfileAsync();
            if (existing != null) return RedirectToAction("EditProfile");
            return View(new NGOProfileViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(NGOProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _db.NGOProfiles.AnyAsync(n => n.RegisterNumber == vm.RegisterNumber))
            {
                ModelState.AddModelError("RegisterNumber", "This registration number is already registered.");
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            _db.NGOProfiles.Add(new NGOProfile
            {
                UserId = user!.Id,
                OrganizationName = vm.OrganizationName,
                RegisterNumber = vm.RegisterNumber,
                PhoneNumber = vm.PhoneNumber,
                StreetName = vm.StreetName,
                City = vm.City,
                Pincode = vm.Pincode,
                State = vm.State,
                Country = vm.Country,
                Description = vm.Description,
                IsApproved = false
            });
            await _db.SaveChangesAsync();
            TempData["Info"] = "Profile submitted. Awaiting admin approval — you'll be notified once reviewed.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            return View(new NGOProfileViewModel
            {
                OrganizationName = profile.OrganizationName,
                RegisterNumber = profile.RegisterNumber,
                PhoneNumber = profile.PhoneNumber,
                StreetName = profile.StreetName,
                City = profile.City,
                Pincode = profile.Pincode,
                State = profile.State,
                Country = profile.Country,
                Description = profile.Description
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(NGOProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            if (vm.RegisterNumber != profile.RegisterNumber &&
                await _db.NGOProfiles.AnyAsync(n => n.RegisterNumber == vm.RegisterNumber))
            {
                ModelState.AddModelError("RegisterNumber", "This registration number is already registered.");
                return View(vm);
            }

            profile.OrganizationName = vm.OrganizationName;
            profile.RegisterNumber = vm.RegisterNumber;
            profile.PhoneNumber = vm.PhoneNumber;
            profile.StreetName = vm.StreetName;
            profile.City = vm.City;
            profile.Pincode = vm.Pincode;
            profile.State = vm.State;
            profile.Country = vm.Country;
            profile.Description = vm.Description;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // ─── Bank Details — Razorpay Route onboarding ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> BankDetails()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");
            if (!profile.IsApproved)
            {
                TempData["Error"] = "Your NGO must be approved before adding bank details.";
                return RedirectToAction("Dashboard");
            }

            return View(new BankDetailsViewModel
            {
                BankAccountHolderName = profile.BankAccountHolderName ?? profile.OrganizationName,
                BankAccountNumber = profile.BankAccountNumber ?? "",
                BankIfscCode = profile.BankIfscCode ?? ""
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BankDetails(BankDetailsViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var user = await _userManager.GetUserAsync(User);

            profile.BankAccountHolderName = vm.BankAccountHolderName;
            profile.BankAccountNumber = vm.BankAccountNumber;
            profile.BankIfscCode = vm.BankIfscCode;
            profile.BankDetailsSubmitted = true;

            // Attempt to create a Razorpay Route linked account so future
            // donations split directly to this NGO's bank account. If Route
            // is not yet enabled on the platform account, this safely returns
            // null and donations still work via the standard flow.
            var accountId = await _razorpay.CreateLinkedAccountAsync(
                profile.OrganizationName, user!.Email ?? "ngo@smiledesk.in", profile.PhoneNumber,
                vm.BankAccountHolderName, vm.BankAccountNumber, vm.BankIfscCode);

            profile.RazorpayAccountId = accountId;
            await _db.SaveChangesAsync();

            TempData["Success"] = accountId != null
                ? "Bank details saved. Future donations will be sent directly to your account."
                : "Bank details saved. Direct payouts are pending Razorpay Route approval — funds will be settled to you manually until then.";
            return RedirectToAction("Dashboard");
        }

        // ─── Events — Create ──────────────────────────────────────────────────
        [HttpGet] public IActionResult PostEvent() => View(new NGOEventViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEvent(NGOEventViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null || !profile.IsApproved)
            {
                TempData["Error"] = "Your NGO must be approved before posting events.";
                return RedirectToAction("Dashboard");
            }

            string? imagePath = await SaveImageAsync(vm.Image, "events");

            _db.NGOEvents.Add(new NGOEvent
            {
                NGOProfileId = profile.Id,
                Title = vm.Title,
                Description = vm.Description,
                Category = vm.Category,
                FundingGoal = vm.FundingGoal,
                EventDate = DateTime.SpecifyKind(vm.EventDate, DateTimeKind.Utc),
                ImagePath = imagePath
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Event published. Donors can now see it.";
            return RedirectToAction("MyEvents");
        }

        // ─── Events — Edit ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var ev = await _db.NGOEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.NGOProfileId == profile.Id && !e.IsDeleted);
            if (ev == null) return NotFound();

            return View(new NGOEventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Category = ev.Category,
                FundingGoal = ev.FundingGoal,
                EventDate = ev.EventDate
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(NGOEventViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var ev = await _db.NGOEvents
                .FirstOrDefaultAsync(e => e.Id == vm.Id && e.NGOProfileId == profile.Id && !e.IsDeleted);
            if (ev == null) return NotFound();

            ev.Title = vm.Title;
            ev.Description = vm.Description;
            ev.Category = vm.Category;
            ev.FundingGoal = vm.FundingGoal;
            ev.EventDate = DateTime.SpecifyKind(vm.EventDate, DateTimeKind.Utc); ev.UpdatedOn = DateTime.UtcNow;

            if (vm.Image != null && vm.Image.Length > 0)
                ev.ImagePath = await SaveImageAsync(vm.Image, "events") ?? ev.ImagePath;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Event updated successfully.";
            return RedirectToAction("MyEvents");
        }

        // ─── Events — Delete (soft) ───────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var ev = await _db.NGOEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.NGOProfileId == profile.Id);
            if (ev == null) return NotFound();

            if (ev.FundsRaised > 0)
            {
                // Don't delete events with donation history — deactivate instead
                ev.IsActive = false;
                await _db.SaveChangesAsync();
                TempData["Success"] = "This event has received donations, so it has been closed instead of deleted, to preserve donor records.";
                return RedirectToAction("MyEvents");
            }

            ev.IsDeleted = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Event removed.";
            return RedirectToAction("MyEvents");
        }

        public async Task<IActionResult> MyEvents()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var events = await _db.NGOEvents
                .Where(e => e.NGOProfileId == profile.Id && !e.IsDeleted)
                .OrderByDescending(e => e.PostedOn).ToListAsync();
            return View(events);
        }

        // ─── Item Requests ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RequestItem(int itemId, string message)
        {
            var profile = await GetProfileAsync();
            if (profile == null || !profile.IsApproved)
            {
                TempData["Error"] = "Your NGO must be approved to request items.";
                return RedirectToAction("Dashboard");
            }

            bool alreadyRequested = await _db.ItemRequests
                .AnyAsync(r => r.NGOProfileId == profile.Id
                            && r.DonationItemId == itemId
                            && r.Status == ItemRequestStatus.Pending);
            if (alreadyRequested)
            {
                TempData["Error"] = "You have already sent a request for this item.";
                return RedirectToAction("BrowseItems");
            }

            _db.ItemRequests.Add(new ItemRequest
            {
                NGOProfileId = profile.Id,
                DonationItemId = itemId,
                Message = string.IsNullOrWhiteSpace(message) ? "We would like to receive this item for our cause." : message,
                Status = ItemRequestStatus.Pending
            });

            var item = await _db.DonationItems.FindAsync(itemId);
            if (item != null) item.Status = DonationItemStatus.Requested;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Request sent. The donor will review it shortly.";
            return RedirectToAction("MyRequests");
        }

        // ─── Withdraw a pending request ────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawRequest(int requestId)
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var request = await _db.ItemRequests
                .Include(r => r.DonationItem)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.NGOProfileId == profile.Id);
            if (request == null) return NotFound();

            if (request.Status != ItemRequestStatus.Pending)
            {
                TempData["Error"] = "Only pending requests can be withdrawn.";
                return RedirectToAction("MyRequests");
            }

            if (request.DonationItem != null)
                request.DonationItem.Status = DonationItemStatus.Available;

            _db.ItemRequests.Remove(request);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Request withdrawn.";
            return RedirectToAction("MyRequests");
        }

        public async Task<IActionResult> MyRequests()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var requests = await _db.ItemRequests
                .Include(r => r.DonationItem)
                .ThenInclude(d => d!.DonorProfile)
                .Where(r => r.NGOProfileId == profile.Id)
                .OrderByDescending(r => r.RequestedOn)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> BrowseItems()
        {
            var items = await _db.DonationItems
                .Include(d => d.DonorProfile)
                .Where(d => d.Status == DonationItemStatus.Available && !d.IsDeleted)
                .OrderByDescending(d => d.PostedOn)
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> ReceivedDonations()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            var donations = await _db.MoneyDonations
                .Include(m => m.DonorProfile)
                .Include(m => m.NGOEvent)
                .Where(m => m.NGOProfileId == profile.Id && m.Status == PaymentStatus.Success)
                .OrderByDescending(m => m.DonatedOn)
                .ToListAsync();
            return View(donations);
        }

        // ─── Browse Donors who have supported this NGO ────────────────────────
        public async Task<IActionResult> BrowseDonors()
        {
            var profile = await GetProfileAsync();
            if (profile == null) return RedirectToAction("CreateProfile");

            // Donors who have either donated money to this NGO or had an
            // accepted/picked-up item request with this NGO. We surface only
            // donors with an existing relationship to this NGO, not a global
            // directory, to keep donor contact details from being broadly exposed.
            var donorIdsFromMoney = _db.MoneyDonations
                .Where(m => m.NGOProfileId == profile.Id && m.Status == PaymentStatus.Success)
                .Select(m => m.DonorProfileId);

            var donorIdsFromItems = _db.ItemRequests
                .Where(r => r.NGOProfileId == profile.Id &&
                            (r.Status == ItemRequestStatus.Accepted || r.Status == ItemRequestStatus.PickedUp))
                .Select(r => r.DonationItem!.DonorProfileId);

            var donorIds = await donorIdsFromMoney.Union(donorIdsFromItems).Distinct().ToListAsync();

            var donors = await _db.DonorProfiles
                .Where(d => donorIds.Contains(d.Id))
                .Include(d => d.MoneyDonations.Where(m => m.NGOProfileId == profile.Id && m.Status == PaymentStatus.Success))
                .Include(d => d.DonationItems.Where(i => i.Requests.Any(r => r.NGOProfileId == profile.Id)))
                .ToListAsync();

            return View(donors);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private async Task<string?> SaveImageAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;

            var dir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subfolder}/{fileName}";
        }
    }
}
