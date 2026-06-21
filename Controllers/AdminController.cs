using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmileDesk.Data;
using SmileDesk.Models;
using SmileDesk.ViewModels;

namespace SmileDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db; _userManager = um;
        }

        // ─── Dashboard ────────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalDonors = await _db.DonorProfiles.CountAsync(),
                TotalNGOs = await _db.NGOProfiles.CountAsync(n => n.IsApproved),
                PendingNGOApprovals = await _db.NGOProfiles.CountAsync(n => !n.IsApproved),
                TotalDonationItems = await _db.DonationItems.CountAsync(),
                TotalFundsRaised = await _db.MoneyDonations
                    .Where(m => m.Status == PaymentStatus.Success)
                    .SumAsync(m => (decimal?)m.Amount) ?? 0,
                TotalMoneyDonations = await _db.MoneyDonations
                    .CountAsync(m => m.Status == PaymentStatus.Success),
                PendingNGOs = await _db.NGOProfiles
                    .Include(n => n.User)
                    .Where(n => !n.IsApproved)
                    .OrderBy(n => n.CreatedOn).ToListAsync(),
                RecentDonations = await _db.MoneyDonations
                    .Include(m => m.DonorProfile)
                    .Include(m => m.NGOProfile)
                    .Where(m => m.Status == PaymentStatus.Success)
                    .OrderByDescending(m => m.DonatedOn)
                    .Take(10).ToListAsync()
            };
            return View(vm);
        }

        // ─── NGO Management ───────────────────────────────────────────────────
        public async Task<IActionResult> NGOs()
        {
            var ngos = await _db.NGOProfiles.Include(n => n.User)
                .OrderByDescending(n => n.CreatedOn).ToListAsync();
            return View(ngos);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveNGO(int id)
        {
            var ngo = await _db.NGOProfiles.FindAsync(id);
            if (ngo == null) return NotFound();
            ngo.IsApproved = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{ngo.OrganizationName} has been approved!";
            return RedirectToAction("NGOs");
        }

        [HttpPost]
        public async Task<IActionResult> RejectNGO(int id)
        {
            var ngo = await _db.NGOProfiles.FindAsync(id);
            if (ngo == null) return NotFound();

            // Deactivate the user
            if (ngo.User != null)
            {
                ngo.User.IsActive = false;
                await _userManager.UpdateAsync(ngo.User);
            }
            _db.NGOProfiles.Remove(ngo);
            await _db.SaveChangesAsync();
            TempData["Success"] = "NGO registration rejected and removed.";
            return RedirectToAction("NGOs");
        }

        // ─── Donors ───────────────────────────────────────────────────────────
        public async Task<IActionResult> Donors()
        {
            var donors = await _db.DonorProfiles.Include(d => d.User)
                .OrderByDescending(d => d.Id).ToListAsync();
            return View(donors);
        }

        // ─── Donations Overview ───────────────────────────────────────────────
        public async Task<IActionResult> Donations()
        {
            var donations = await _db.MoneyDonations
                .Include(m => m.DonorProfile)
                .Include(m => m.NGOProfile)
                .Include(m => m.NGOEvent)
                .OrderByDescending(m => m.DonatedOn).ToListAsync();
            return View(donations);
        }

        // ─── Item Donations Overview ──────────────────────────────────────────
        public async Task<IActionResult> ItemDonations()
        {
            var items = await _db.DonationItems
                .Where(d => !d.IsDeleted)
                .Include(d => d.DonorProfile)
                .Include(d => d.Requests).ThenInclude(r => r.NGOProfile)
                .OrderByDescending(d => d.PostedOn).ToListAsync();
            return View(items);
        }

        // ─── Events Overview ──────────────────────────────────────────────────
        public async Task<IActionResult> Events()
        {
            var events = await _db.NGOEvents
                .Where(e => !e.IsDeleted)
                .Include(e => e.NGOProfile)
                .OrderByDescending(e => e.PostedOn).ToListAsync();
            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEvent(int id)
        {
            var ev = await _db.NGOEvents.FindAsync(id);
            if (ev == null) return NotFound();
            ev.IsActive = !ev.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Event {(ev.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction("Events");
        }
    }
}
