using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmileDesk.Models
{
    // ─── Donor Profile ───────────────────────────────────────────────────────
    public class DonorProfile
    {
        [Key] public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")] public ApplicationUser? User { get; set; }

        [Required, MaxLength(100)] public string DonorName { get; set; } = string.Empty;
        public ContributionType ContributionType { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Required, MaxLength(15)] public string MobileNumber { get; set; } = string.Empty;
        [MaxLength(10)] public string? DoorNo { get; set; }
        [MaxLength(100)] public string StreetName { get; set; } = string.Empty;
        [MaxLength(50)] public string City { get; set; } = string.Empty;
        [MaxLength(50)] public string State { get; set; } = string.Empty;
        [MaxLength(50)] public string Country { get; set; } = string.Empty;

        public ICollection<DonationItem> DonationItems { get; set; } = new List<DonationItem>();
        public ICollection<MoneyDonation> MoneyDonations { get; set; } = new List<MoneyDonation>();
    }

    public enum ContributionType { Money = 1, Items = 2, Both = 3 }

    // ─── NGO Profile ─────────────────────────────────────────────────────────
    public class NGOProfile
    {
        [Key] public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")] public ApplicationUser? User { get; set; }

        [Required, MaxLength(150)] public string OrganizationName { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string RegisterNumber { get; set; } = string.Empty;
        [Required, MaxLength(15)] public string PhoneNumber { get; set; } = string.Empty;
        [MaxLength(100)] public string StreetName { get; set; } = string.Empty;
        [MaxLength(50)] public string City { get; set; } = string.Empty;
        [MaxLength(6)] public string Pincode { get; set; } = string.Empty;
        [MaxLength(50)] public string State { get; set; } = string.Empty;
        [MaxLength(50)] public string Country { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        [MaxLength(500)] public string? Description { get; set; }

        // ── Razorpay Route (split payments) ──────────────────────────────────
        // When an NGO submits bank/KYC details, a Razorpay Linked Account is
        // created and its id stored here. Donations are then split directly
        // to this linked account instead of pooling in the platform account.
        [MaxLength(100)] public string? RazorpayAccountId { get; set; }
        public bool BankDetailsSubmitted { get; set; } = false;
        [MaxLength(100)] public string? BankAccountHolderName { get; set; }
        [MaxLength(20)]  public string? BankAccountNumber { get; set; }
        [MaxLength(15)]  public string? BankIfscCode { get; set; }

        public ICollection<NGOEvent> Events { get; set; } = new List<NGOEvent>();
        public ICollection<ItemRequest> ItemRequests { get; set; } = new List<ItemRequest>();
        public ICollection<MoneyDonation> MoneyDonations { get; set; } = new List<MoneyDonation>();
    }

    // ─── Donation Items (Donor posts items) ──────────────────────────────────
    public class DonationItem
    {
        [Key] public int Id { get; set; }
        public int DonorProfileId { get; set; }
        [ForeignKey("DonorProfileId")] public DonorProfile? DonorProfile { get; set; }

        [Required, MaxLength(100)] public string Title { get; set; } = string.Empty;
        public ItemCategory Category { get; set; }
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
        public ItemCondition Condition { get; set; }
        public DonationItemStatus Status { get; set; } = DonationItemStatus.Available;
        public DateTime PostedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        [MaxLength(200)] public string? ImagePath { get; set; }

        // Soft delete — keeps history intact for requests already made against it
        public bool IsDeleted { get; set; } = false;

        public ICollection<ItemRequest> Requests { get; set; } = new List<ItemRequest>();
    }

    public enum ItemCategory { Clothes = 1, Furniture = 2, Electronics = 3, Books = 4, Food = 5, Medical = 6, Other = 7 }
    public enum ItemCondition { New = 1, Good = 2, Used = 3 }
    public enum DonationItemStatus { Available = 1, Requested = 2, Accepted = 3, PickedUp = 4, Donated = 5 }

    // ─── Item Requests (NGO requests donor items) ─────────────────────────────
    public class ItemRequest
    {
        [Key] public int Id { get; set; }
        public int NGOProfileId { get; set; }
        [ForeignKey("NGOProfileId")] public NGOProfile? NGOProfile { get; set; }
        public int DonationItemId { get; set; }
        [ForeignKey("DonationItemId")] public DonationItem? DonationItem { get; set; }

        [MaxLength(500)] public string Message { get; set; } = string.Empty;
        public ItemRequestStatus Status { get; set; } = ItemRequestStatus.Pending;
        public DateTime RequestedOn { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedOn { get; set; }
        // Set once the NGO confirms physical pickup of the item from the donor
        public DateTime? PickedUpOn { get; set; }
    }

    public enum ItemRequestStatus { Pending = 1, Accepted = 2, Rejected = 3, PickedUp = 4 }

    // ─── NGO Events ───────────────────────────────────────────────────────────
    public class NGOEvent
    {
        [Key] public int Id { get; set; }
        public int NGOProfileId { get; set; }
        [ForeignKey("NGOProfileId")] public NGOProfile? NGOProfile { get; set; }

        [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
        [MaxLength(1000)] public string Description { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? FundingGoal { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal FundsRaised { get; set; } = 0;
        public DateTime EventDate { get; set; }
        public DateTime PostedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public bool IsActive { get; set; } = true;
        [MaxLength(200)] public string? ImagePath { get; set; }

        // Soft delete — preserves donation history tied to this event
        public bool IsDeleted { get; set; } = false;

        public ICollection<MoneyDonation> Donations { get; set; } = new List<MoneyDonation>();
    }

    public enum EventCategory { Education = 1, Food = 2, Medical = 3, Infrastructure = 4, Clothing = 5, Other = 6 }

    // ─── Money Donations ──────────────────────────────────────────────────────
    public class MoneyDonation
    {
        [Key] public int Id { get; set; }
        public int DonorProfileId { get; set; }
        [ForeignKey("DonorProfileId")] public DonorProfile? DonorProfile { get; set; }
        public int NGOProfileId { get; set; }
        [ForeignKey("NGOProfileId")] public NGOProfile? NGOProfile { get; set; }
        public int? NGOEventId { get; set; }
        [ForeignKey("NGOEventId")] public NGOEvent? NGOEvent { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        [MaxLength(500)] public string? Message { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
        public DateTime DonatedOn { get; set; } = DateTime.UtcNow;

        // True once the payment was split via Razorpay Route to the NGO's
        // linked account, rather than just sitting in the platform balance.
        public bool RoutedToNgo { get; set; } = false;
        public string? RazorpayTransferId { get; set; }
    }

    public enum PaymentStatus { Pending = 1, Success = 2, Failed = 3 }

    // ─── Country / State / City Lookup ────────────────────────────────────────
    public class Country
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(100)] public string CountryName { get; set; } = string.Empty;
        [MaxLength(10)] public string CountryCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<State> States { get; set; } = new List<State>();
    }

    public class State
    {
        [Key] public int Id { get; set; }
        public int CountryId { get; set; }
        [ForeignKey("CountryId")] public Country? Country { get; set; }
        [Required, MaxLength(100)] public string StateName { get; set; } = string.Empty;
        [MaxLength(10)] public string StateCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<City> Cities { get; set; } = new List<City>();
    }

    public class City
    {
        [Key] public int Id { get; set; }
        public int StateId { get; set; }
        [ForeignKey("StateId")] public State? StateObj { get; set; }
        [Required, MaxLength(100)] public string CityName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
