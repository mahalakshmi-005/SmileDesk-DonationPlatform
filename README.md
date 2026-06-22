<div align="center">

# 💚 Smile Desk

**Where generosity meets verified impact.**

A full-stack donation platform connecting donors and NGOs across India —
built end-to-end with ASP.NET Core, Razorpay, and PostgreSQL.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Razorpay](https://img.shields.io/badge/Payments-Razorpay-0C2451)](https://razorpay.com/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql)](https://www.postgresql.org/)
[![Deployed on Render](https://img.shields.io/badge/Deployed-Render-46E3B7?logo=render)](https://render.com/)

</div>

---

## 🌍 Live

**[smiledesk-donationplatform.onrender.com](https://smiledesk-donationplatform.onrender.com)**

*(Free-tier hosting — first load after idle may take ~30s to wake up.)*

---

## ✨ What it does

Smile Desk solves a simple but real problem: donors want to give, NGOs need help,
and there's rarely a trusted bridge between them. This platform is that bridge.

| | |
|---|---|
| 💳 | Donors fund verified NGO events directly through **Razorpay** |
| 🔀 | **Razorpay Route** splits payments straight to an NGO's bank account |
| 📦 | Donors list physical items; NGOs request, accept, and confirm handover |
| 🏢 | Every NGO is **admin-approved** before it can raise funds |
| 🔍 | Donors browse a live NGO directory; NGOs see who's supported them |
| 🛠️ | Full CRUD — edit or retire any profile, item, or event you've posted |

---

## 🧱 Built with

`ASP.NET Core 8 MVC` · `C#` · `Entity Framework Core` · `PostgreSQL` ·
`ASP.NET Identity` · `Razorpay API` · `Docker` · `Custom SVG icon system`

No template UI kits, no font-icon CDNs — the entire design system (theme,
animations, icon set) is hand-built and dependency-free.

---

## 🚀 Run it locally

\`\`\`bash
git clone https://github.com/mahalakshmi-005/SmileDesk-DonationPlatform.git
cd SmileDesk-DonationPlatform

dotnet user-secrets init
dotnet user-secrets set "Razorpay:KeyId" "rzp_test_xxxxxxxx"
dotnet user-secrets set "Razorpay:KeySecret" "xxxxxxxxxxxxxx"

dotnet ef database update
dotnet run
\`\`\`

---

## 👩‍💻 Author

**Maha Lakshmi Kannan** — B.Sc Computer Science, Jayaraj Annapackiam College For Womens (Autonomous), Periyakulam
Final-year project, 2025.

<div align="center"><sub>Built with patience, a lot of debugging, and a genuine belief that giving should be easy.</sub></div>
