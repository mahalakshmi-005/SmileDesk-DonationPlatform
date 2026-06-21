using System.ComponentModel.DataAnnotations;
using SmileDesk.Models;

namespace SmileDesk.ViewModels
{
    // ─── Auth ─────────────────────────────────────────────────────────────────
    public class RegisterViewModel
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare("Password")] public string ConfirmPassword { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = "Donor"; // Donor or NGO
    }

    public class LoginViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    // ─── Donor ────────────────────────────────────────────────────────────────
    public class DonorProfileViewModel
    {
        [Required] public string DonorName { get; set; } = string.Empty;
        [Required] public ContributionType ContributionType { get; set; }
        [Required] public DateTime DateOfBirth { get; set; }
        [Required, Phone] public string MobileNumber { get; set; } = string.Empty;
        public string? DoorNo { get; set; }
        [Required] public string StreetName { get; set; } = string.Empty;
        [Required] public string City { get; set; } = string.Empty;
        [Required] public string State { get; set; } = string.Empty;
        [Required] public string Country { get; set; } = "India";
    }

    // ─── NGO ──────────────────────────────────────────────────────────────────
    public class NGOProfileViewModel
    {
        [Required] public string OrganizationName { get; set; } = string.Empty;
        [Required] public string RegisterNumber { get; set; } = string.Empty;
        [Required, Phone] public string PhoneNumber { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        [Required, MaxLength(6)] public string Pincode { get; set; } = string.Empty;
        [Required] public string State { get; set; } = string.Empty;
        [Required] public string Country { get; set; } = "India";
        public string? Description { get; set; }
    }

    // ─── Donation Item ─────────────────────────────────────────────────────────
    public class DonationItemViewModel
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public ItemCategory Category { get; set; }
        [Required] public string Description { get; set; } = string.Empty;
        [Required] public ItemCondition Condition { get; set; }
        public IFormFile? Image { get; set; }
    }

    // ─── NGO Event ────────────────────────────────────────────────────────────
    public class NGOEventViewModel
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;
        [Required] public EventCategory Category { get; set; }
        [Range(100, 10000000)] public decimal? FundingGoal { get; set; }
        [Required] public DateTime EventDate { get; set; }
        public IFormFile? Image { get; set; }
    }

    // ─── NGO Bank Details (Razorpay Route onboarding) ─────────────────────────
    public class BankDetailsViewModel
    {
        [Required, MaxLength(100)] public string BankAccountHolderName { get; set; } = string.Empty;
        [Required, MaxLength(20)]  public string BankAccountNumber { get; set; } = string.Empty;
        [Required, MaxLength(15), RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Enter a valid IFSC code, e.g. SBIN0001234")]
        public string BankIfscCode { get; set; } = string.Empty;
    }

    // ─── Money Donation ────────────────────────────────────────────────────────
    public class MoneyDonationViewModel
    {
        public int NGOProfileId { get; set; }
        public int? NGOEventId { get; set; }
        [Required, Range(10, 1000000)] public decimal Amount { get; set; }
        public string? Message { get; set; }
        public string? NGOName { get; set; }
        public string? EventTitle { get; set; }
    }

    public class PaymentCallbackViewModel
    {
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
        public int DonationId { get; set; }
    }

    // ─── Dashboard Summary ─────────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public int TotalDonors { get; set; }
        public int TotalNGOs { get; set; }
        public int PendingNGOApprovals { get; set; }
        public int TotalDonationItems { get; set; }
        public decimal TotalFundsRaised { get; set; }
        public int TotalMoneyDonations { get; set; }
        public List<NGOProfile> PendingNGOs { get; set; } = new();
        public List<MoneyDonation> RecentDonations { get; set; } = new();
    }

    public class DonorDashboardViewModel
    {
        public DonorProfile? Profile { get; set; }
        public List<DonationItem> MyItems { get; set; } = new();
        public List<MoneyDonation> MyDonations { get; set; } = new();
        public List<ItemRequest> PendingRequests { get; set; } = new();
        public List<NGOEvent> RecentEvents { get; set; } = new();
        public int TotalItemsDonated { get; set; }
        public decimal TotalMoneyDonated { get; set; }
    }

    public class NGODashboardViewModel
    {
        public NGOProfile? Profile { get; set; }
        public List<NGOEvent> MyEvents { get; set; } = new();
        public List<ItemRequest> MyItemRequests { get; set; } = new();
        public List<MoneyDonation> ReceivedDonations { get; set; } = new();
        public decimal TotalFundsReceived { get; set; }
        public int ItemsReceived { get; set; }
        public bool BankDetailsNeeded { get; set; }
    }
}
