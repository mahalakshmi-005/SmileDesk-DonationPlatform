using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmileDesk.Data;

namespace SmileDesk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDonors = await _db.DonorProfiles.CountAsync();
            ViewBag.TotalNGOs = await _db.NGOProfiles.CountAsync(n => n.IsApproved);
            ViewBag.TotalDonations = await _db.MoneyDonations
                .Where(d => d.Status == SmileDesk.Models.PaymentStatus.Success)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;
            ViewBag.Events = await _db.NGOEvents
                .Include(e => e.NGOProfile)
                .Where(e => e.IsActive && !e.IsDeleted && e.EventDate >= DateTime.UtcNow.Date)
                .OrderBy(e => e.EventDate)
                .Take(6)
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> Events(string? category, int? ngoId)
        {
            var query = _db.NGOEvents.Include(e => e.NGOProfile).Where(e => e.IsActive && !e.IsDeleted);
            if (!string.IsNullOrEmpty(category) &&
                Enum.TryParse<SmileDesk.Models.EventCategory>(category, out var cat))
                query = query.Where(e => e.Category == cat);
            if (ngoId.HasValue)
                query = query.Where(e => e.NGOProfileId == ngoId.Value);

            ViewBag.Category = category;
            ViewBag.NgoFilterName = ngoId.HasValue
                ? (await _db.NGOProfiles.FindAsync(ngoId.Value))?.OrganizationName
                : null;
            return View(await query.OrderByDescending(e => e.PostedOn).ToListAsync());
        }

        public async Task<IActionResult> EventDetail(int id)
        {
            var ev = await _db.NGOEvents.Include(e => e.NGOProfile)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        public async Task<IActionResult> DonationItems(string? category)
        {
            var query = _db.DonationItems
                .Include(d => d.DonorProfile)
                .Where(d => d.Status == SmileDesk.Models.DonationItemStatus.Available && !d.IsDeleted);
            if (!string.IsNullOrEmpty(category) &&
                Enum.TryParse<SmileDesk.Models.ItemCategory>(category, out var cat))
                query = query.Where(d => d.Category == cat);

            ViewBag.Category = category;
            return View(await query.OrderByDescending(d => d.PostedOn).ToListAsync());
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
