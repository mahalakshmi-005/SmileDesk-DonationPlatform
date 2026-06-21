using Microsoft.AspNetCore.Identity;

namespace SmileDesk.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Donor", "NGO", "Admin"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation
        public DonorProfile? DonorProfile { get; set; }
        public NGOProfile? NGOProfile { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
