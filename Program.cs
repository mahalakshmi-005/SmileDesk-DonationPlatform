using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmileDesk.Data;
using SmileDesk.Models;
using SmileDesk.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Razor runtime compilation ────────────────────────────────────────
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// ── SQL Server via connection string ───────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmileDesk")));

// ── Identity (using AddIdentity — no Razor Pages / Identity.UI needed) ─────
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount   = false;
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie settings ────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath       = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan  = TimeSpan.FromDays(7);
});

// ── Custom services ────────────────────────────────────────────────────────
builder.Services.AddHttpClient("Razorpay");
builder.Services.AddScoped<RazorpayService>();

// ── Session ────────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout     = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// No MapRazorPages() needed — we use MVC controllers only
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Seed roles & admin on first run ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdmin(services);
}

app.Run();

// ──────────────────────────────────────────────────────────────────────────
static async Task SeedRolesAndAdmin(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Donor", "NGO" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new ApplicationRole(role));
    }

    // Seed default admin
    var adminEmail = "admin@smiledesk.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            FullName       = "Smile Desk Admin",
            Role           = "Admin",
            IsActive       = true,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
