using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmileDesk.Models;

namespace SmileDesk.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<DonorProfile> DonorProfiles { get; set; }
        public DbSet<NGOProfile> NGOProfiles { get; set; }
        public DbSet<DonationItem> DonationItems { get; set; }
        public DbSet<ItemRequest> ItemRequests { get; set; }
        public DbSet<NGOEvent> NGOEvents { get; set; }
        public DbSet<MoneyDonation> MoneyDonations { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");

            // Indexes
            builder.Entity<DonorProfile>().HasIndex(d => d.UserId).IsUnique();
            builder.Entity<NGOProfile>().HasIndex(n => n.UserId).IsUnique();
            builder.Entity<NGOProfile>().HasIndex(n => n.RegisterNumber).IsUnique();

            // ── Cascade delete fixes ───────────────────────────────────────────
            // SQL Server rejects multiple cascade paths into the same table.
            // ItemRequests and MoneyDonations each reach NGOProfiles through more
            // than one path, so those specific relationships use NoAction instead
            // — the rows are still removed in application code where relevant
            // (e.g. soft-delete), just not automatically by the database.
            builder.Entity<ItemRequest>()
                .HasOne(r => r.NGOProfile)
                .WithMany(n => n.ItemRequests)
                .HasForeignKey(r => r.NGOProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ItemRequest>()
                .HasOne(r => r.DonationItem)
                .WithMany(d => d.Requests)
                .HasForeignKey(r => r.DonationItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MoneyDonation>()
                .HasOne(m => m.DonorProfile)
                .WithMany(d => d.MoneyDonations)
                .HasForeignKey(m => m.DonorProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MoneyDonation>()
                .HasOne(m => m.NGOProfile)
                .WithMany(n => n.MoneyDonations)
                .HasForeignKey(m => m.NGOProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MoneyDonation>()
                .HasOne(m => m.NGOEvent)
                .WithMany(e => e.Donations)
                .HasForeignKey(m => m.NGOEventId)
                .OnDelete(DeleteBehavior.NoAction);

            // Seed Countries & States
            builder.Entity<Country>().HasData(
                new Country { Id = 1, CountryName = "India", CountryCode = "IN", IsActive = true }
            );
            builder.Entity<State>().HasData(
                new State { Id = 1, CountryId = 1, StateName = "Tamil Nadu", StateCode = "TN", IsActive = true },
                new State { Id = 2, CountryId = 1, StateName = "Karnataka", StateCode = "KA", IsActive = true },
                new State { Id = 3, CountryId = 1, StateName = "Kerala", StateCode = "KL", IsActive = true },
                new State { Id = 4, CountryId = 1, StateName = "Andhra Pradesh", StateCode = "AP", IsActive = true },
                new State { Id = 5, CountryId = 1, StateName = "Maharashtra", StateCode = "MH", IsActive = true },
                new State { Id = 6, CountryId = 1, StateName = "Delhi", StateCode = "DL", IsActive = true }
            );
            builder.Entity<City>().HasData(
                new City { Id = 1, StateId = 1, CityName = "Chennai", IsActive = true },
                new City { Id = 2, StateId = 1, CityName = "Coimbatore", IsActive = true },
                new City { Id = 3, StateId = 1, CityName = "Madurai", IsActive = true },
                new City { Id = 4, StateId = 1, CityName = "Salem", IsActive = true },
                new City { Id = 5, StateId = 1, CityName = "Periyakulam", IsActive = true },
                new City { Id = 6, StateId = 2, CityName = "Bangalore", IsActive = true },
                new City { Id = 7, StateId = 3, CityName = "Kochi", IsActive = true },
                new City { Id = 8, StateId = 5, CityName = "Mumbai", IsActive = true },
                new City { Id = 9, StateId = 6, CityName = "New Delhi", IsActive = true }
            );
        }
    }
}
