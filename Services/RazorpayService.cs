using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmileDesk.Services
{
    public class RazorpayService
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly HttpClient _http;

        public RazorpayService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _keyId     = configuration["Razorpay:KeyId"]     ?? throw new Exception("Razorpay KeyId missing");
            _keySecret = configuration["Razorpay:KeySecret"] ?? throw new Exception("Razorpay KeySecret missing");
            _http      = httpClientFactory.CreateClient("Razorpay");
        }

        public string GetKeyId() => _keyId;

        private HttpRequestMessage NewRequest(HttpMethod method, string path, object? payload = null)
        {
            var request = new HttpRequestMessage(method, $"https://api.razorpay.com/v1/{path}");
            if (payload != null)
            {
                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            return request;
        }

        // ── Orders ──────────────────────────────────────────────────────────
        public async Task<string> CreateOrderAsync(decimal amountInRupees, string currency = "INR", string receipt = "", bool transferToNgo = false, string? ngoAccountId = null)
        {
            int amountInPaise = (int)(amountInRupees * 100);

            object payload;
            if (transferToNgo && !string.IsNullOrEmpty(ngoAccountId))
            {
                // Razorpay Route: split the order so funds move directly to the
                // NGO's linked account once payment succeeds. Platform keeps 0
                // commission here — adjust "amount" under transfers to take a cut.
                payload = new
                {
                    amount = amountInPaise,
                    currency,
                    receipt,
                    payment_capture = 1,
                    transfers = new[]
                    {
                        new
                        {
                            account = ngoAccountId,
                            amount = amountInPaise,
                            currency,
                            on_hold = false
                        }
                    }
                };
            }
            else
            {
                payload = new { amount = amountInPaise, currency, receipt, payment_capture = 1 };
            }

            var request  = NewRequest(HttpMethod.Post, "orders", payload);
            var response = await _http.SendAsync(request);
            var body      = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Razorpay order creation failed: {body}");

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        public string CreateOrder(decimal amountInRupees, string currency = "INR", string receipt = "", bool transferToNgo = false, string? ngoAccountId = null)
            => CreateOrderAsync(amountInRupees, currency, receipt, transferToNgo, ngoAccountId).GetAwaiter().GetResult();

        // ── Linked Accounts (Razorpay Route) ───────────────────────────────────
        // Creates a linked sub-account for an NGO so future donations can be
        // split directly to them. Requires Route to be enabled on the platform
        // Razorpay account (test mode supports this without full KYC).
        public async Task<string?> CreateLinkedAccountAsync(string ngoName, string email, string phone,
            string accountHolderName, string accountNumber, string ifsc)
        {
            var payload = new
            {
                email,
                phone,
                type = "route",
                legal_business_name = ngoName,
                business_type = "ngo",
                contact_name = accountHolderName,
                profile = new
                {
                    category = "ngo",
                    subcategory = "charity",
                    addresses = new { registered = new { street1 = "NA", city = "NA", state = "NA", postal_code = "000000", country = "IN" } }
                },
                legal_info = new { },
            };

            try
            {
                var request  = NewRequest(HttpMethod.Post, "accounts", payload);
                var response = await _http.SendAsync(request);
                var body      = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return null;

                using var doc = JsonDocument.Parse(body);
                var accountId = doc.RootElement.GetProperty("id").GetString();

                if (accountId != null)
                    await AddBankAccountAsync(accountId, accountHolderName, accountNumber, ifsc);

                return accountId;
            }
            catch
            {
                // Route requires platform-level approval from Razorpay; if the
                // call fails (e.g. feature not enabled on this account yet),
                // we fall back gracefully — donations still work, just without
                // automatic splitting until Route is approved.
                return null;
            }
        }

        private async Task AddBankAccountAsync(string accountId, string holderName, string accountNumber, string ifsc)
        {
            var payload = new
            {
                account_type = "bank_account",
                bank_account = new { ifsc_code = ifsc, beneficiary_name = holderName, account_number = accountNumber }
            };
            var request = NewRequest(HttpMethod.Post, $"accounts/{accountId}/stakeholders", payload);
            await _http.SendAsync(request);
        }

        // ── Signature verification ─────────────────────────────────────────────
        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            try
            {
                string message = $"{orderId}|{paymentId}";
                using var hmac  = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
                byte[] hash     = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                string expected = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }
}
