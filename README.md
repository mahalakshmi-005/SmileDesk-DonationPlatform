# Smile Desk — Full Stack ASP.NET Core MVC Web App

A donation platform connecting Donors and NGOs, with Razorpay payment integration
including split payments (Razorpay Route) directly to NGO bank accounts.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 MVC + C# |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB for dev) |
| Auth | ASP.NET Core Identity + Roles |
| Payments | Razorpay (Orders API + Route for split payouts) |
| Frontend | Bootstrap 5 + Tabler Icons |

---

## What's new in this build

- **Full CRUD** — Donors and NGOs can edit/delete their own profiles, items, and
  events (soft-delete preserves history where donations/requests already exist).
- **Item handover workflow** — when a donor accepts an NGO's request, the item
  moves to "Accepted", and the donor explicitly confirms physical handover once
  the NGO has picked it up, closing the loop that used to stop at "Accepted".
- **Razorpay Route** — approved NGOs can submit bank details; if Route is enabled
  on the platform Razorpay account, future donations are split and sent directly
  to the NGO's linked account instead of pooling in the platform balance. If Route
  isn't approved yet, donations still work normally and are settled manually.
- **Professional icon set** — Tabler Icons throughout, no emoji.
- **Scroll animations** — sections fade/rise into view as you scroll.

---

## ⚠️ IMPORTANT — Database reset required

This version adds new columns (`IsDeleted`, `RazorpayAccountId`, `BankAccountNumber`,
etc.) and a new `PickedUp` status. Your existing migration is now out of date.

In **Package Manager Console**:

```powershell
Remove-Migration
```

If that fails or there's nothing to remove, just delete everything inside the
`Migrations` folder manually, then:

```powershell
Add-Migration InitialCreate
Update-Database
```

If `Update-Database` fails with a "database already exists" type conflict, the
simplest fix for a project still in development is to drop and recreate it:

```powershell
Drop-Database
Update-Database
```

---

## Setup Instructions

### 1. Prerequisites
- Visual Studio 2022, .NET 8 SDK, SQL Server / LocalDB

### 2. Razorpay keys (User Secrets — never commit these)

```powershell
dotnet user-secrets init
dotnet user-secrets set "Razorpay:KeyId" "rzp_test_xxxxxxxxxxxx"
dotnet user-secrets set "Razorpay:KeySecret" "xxxxxxxxxxxxxxxxxxxx"
```

`appsettings.json` keeps empty placeholders — User Secrets override them locally
and are never pushed to GitHub.

### 3. Database

```powershell
Add-Migration InitialCreate
Update-Database
```

### 4. Run

Press `F5`. Default admin: `admin@smiledesk.com` / `Admin@123`

---

## Razorpay Route — enabling direct NGO payouts

Route requires approval from Razorpay at the account level (KYC + business
verification). Until it's approved on your Razorpay account:

- NGOs can still submit bank details from their dashboard.
- The app safely falls back to standard order creation — donations work exactly
  as before, just without automatic splitting.
- Once Route is approved, no code changes are needed — `RazorpayService` already
  creates linked accounts and routes new orders automatically.

---

## Known limitations (documented honestly)

- **Email verification is not implemented.** Accounts are created with
  `EmailConfirmed = true` automatically for ease of testing. Before any real
  deployment, add email confirmation (OTP or link) so unverified addresses can't
  log in.
- **Test Mode only.** Razorpay is running in test mode. Going live requires
  switching to live API keys after Razorpay completes KYC for the platform account.

---

## Project structure

```
SmileDesk/
├── Controllers/        AccountController, HomeController, DonorController,
│                        NGOController, AdminController — full CRUD actions
├── Models/              ApplicationUser, DomainModels (with soft-delete + Route fields)
├── ViewModels/           Including BankDetailsViewModel for Route onboarding
├── Data/                 ApplicationDbContext with cascade-delete fixes
├── Services/             RazorpayService — orders, linked accounts, transfers, signature
└── Views/                Home, Account, Donor, NGO, Admin — Tabler icons + animations
```

---

*Built by Maha Lakshmi Kannan — UG Final Year Project, Panimalar Engineering College*
