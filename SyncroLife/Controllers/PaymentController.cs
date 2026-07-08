using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Models;
using System.Text.Json;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly SyncroLifeDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(SyncroLifeDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.SubscriptionPlans
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.Price)
                .Select(p => new
                {
                    p.PlanId,
                    p.PlanName,
                    p.Description,
                    p.Price,
                    p.DurationDays,
                    p.Features
                })
                .ToListAsync();

            return Ok(plans);
        }

        [HttpPost("create-payment-link")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentRequest request)
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized("User ID not found in token.");
            }

            var userId = Guid.Parse(userIdClaim);
            var plan = await _context.SubscriptionPlans.FindAsync(request.PlanId);
            if (plan == null)
            {
                return BadRequest("Invalid plan ID.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Generate unique numeric order code (32-bit safe: 7 digits of timestamp in seconds + 2 random digits)
            long orderCode = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 10000000) * 100 + new Random().Next(10, 99);

            // Create a pending subscription
            var userSub = new UserSubscription
            {
                UserSubId = Guid.NewGuid(),
                UserId = userId,
                PlanId = plan.PlanId,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(plan.DurationDays)),
                Status = "pending",
                ScanCount = 0,
                AutoRenew = plan.PlanName.Equals("free", StringComparison.OrdinalIgnoreCase),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.UserSubscriptions.AddAsync(userSub);

            // Create a pending payment log
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                UserSubId = userSub.UserSubId,
                Amount = plan.Price,
                PaymentMethod = "PayOS",
                Status = "pending",
                TransactionId = orderCode.ToString(),
                RetryCount = 0,
                PaymentDate = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Read PayOS configuration
            var payOSSection = _configuration.GetSection("PayOS");
            var clientId = payOSSection["ClientId"] ?? string.Empty;
            var apiKey = payOSSection["ApiKey"] ?? string.Empty;
            var checksumKey = payOSSection["ChecksumKey"] ?? string.Empty;
            var webhookUrl = payOSSection["WebhookUrl"];

            try
            {
                // Create real PayOS checkout link using PayOSClient
                var payOS = new PayOSClient(clientId, apiKey, checksumKey);

                // Confirm/register Webhook URL programmatically if configured with a public URL
                if (!string.IsNullOrEmpty(webhookUrl) && (webhookUrl.StartsWith("https") || webhookUrl.StartsWith("http")))
                {
                    try
                    {
                        await payOS.Webhooks.ConfirmAsync(webhookUrl);
                    }
                    catch (Exception webhookEx)
                    {
                        Console.WriteLine("PayOS Webhook registration failed: " + webhookEx.Message);
                    }
                }

                var scheme = Request.Scheme;
                var host = Request.Host;
                var pathBase = Request.PathBase;

                // Standard return and cancel URLs
                string returnUrl = $"{scheme}://{host}{pathBase}/api/Payment/mock-checkout?userId={userId}&planId={plan.PlanId}&orderCode={orderCode}&status=success";
                string cancelUrl = $"{scheme}://{host}{pathBase}/api/Payment/mock-checkout?userId={userId}&planId={plan.PlanId}&orderCode={orderCode}&status=cancelled";

                // Create CreatePaymentLinkRequest object for SDK
                var paymentData = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (int)plan.Price,
                    Description = $"Goi {plan.PlanName} - SyncroLife",
                    Items = new List<PaymentLinkItem> { 
                        new PaymentLinkItem {
                            Name = plan.PlanName,
                            Quantity = 1,
                            Price = (int)plan.Price
                        } 
                    },
                    CancelUrl = cancelUrl,
                    ReturnUrl = returnUrl
                };

                CreatePaymentLinkResponse createPaymentResult = await payOS.PaymentRequests.CreateAsync(paymentData);
                return Ok(new { 
                    checkoutUrl = createPaymentResult.CheckoutUrl, 
                    qrCode = createPaymentResult.QrCode,
                    isMock = false 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to generate PayOS link: " + ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] Webhook webhookBody)
        {
            var payOSSection = _configuration.GetSection("PayOS");
            var clientId = payOSSection["ClientId"] ?? string.Empty;
            var apiKey = payOSSection["ApiKey"] ?? string.Empty;
            var checksumKey = payOSSection["ChecksumKey"] ?? string.Empty;

            if (string.IsNullOrEmpty(clientId) || clientId.StartsWith("YOUR_"))
            {
                return BadRequest("Webhook endpoints are not active in mock mode.");
            }

            try
            {
                var payOS = new PayOSClient(clientId, apiKey, checksumKey);
                WebhookData verifiedData = await payOS.Webhooks.VerifyAsync(webhookBody);

                long orderCode = verifiedData.OrderCode;
                string transactionId = orderCode.ToString();

                var payment = await _context.Payments
                    .Include(p => p.UserSub)
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                if (payment != null)
                {
                    if (webhookBody.Code == "00") // PayOS Success code
                    {
                        payment.Status = "success";
                        payment.PaymentDate = DateTime.UtcNow;
                        payment.UpdatedAt = DateTime.UtcNow;

                        // Activate this subscription
                        payment.UserSub.Status = "active";
                        payment.UserSub.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
                        var plan = await _context.SubscriptionPlans.FindAsync(payment.UserSub.PlanId);
                        int duration = plan?.DurationDays ?? 30;
                        payment.UserSub.EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(duration));
                        payment.UserSub.UpdatedAt = DateTime.UtcNow;

                        // Deactivate other active subscriptions for this user
                        var otherActiveSubs = await _context.UserSubscriptions
                            .Where(s => s.UserId == payment.UserSub.UserId && s.UserSubId != payment.UserSubId && s.Status == "active")
                            .ToListAsync();

                        foreach (var sub in otherActiveSubs)
                        {
                            sub.Status = "inactive";
                            sub.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        payment.Status = "failed";
                        payment.ErrorMessage = webhookBody.Description;
                        payment.UpdatedAt = DateTime.UtcNow;

                        payment.UserSub.Status = "cancelled";
                        payment.UserSub.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Webhook processed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Webhook verification failed: " + ex.Message });
            }
        }

        [HttpGet("check-status")]
        [Authorize]
        public async Task<IActionResult> CheckPaymentStatus([FromQuery] Guid userId)
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || Guid.Parse(userIdClaim) != userId)
            {
                return Unauthorized("Unauthorized access to payment status.");
            }
            var pendingPayment = await _context.Payments
                .Include(p => p.UserSub)
                    .ThenInclude(us => us.Plan)
                .Where(p => p.UserSub.UserId == userId && p.Status == "pending")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pendingPayment == null)
            {
                var latestPayment = await _context.Payments
                    .Include(p => p.UserSub)
                        .ThenInclude(us => us.Plan)
                    .Where(p => p.UserSub.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestPayment != null && latestPayment.Status == "success")
                {
                    return Ok(new { status = "success", plan = latestPayment.UserSub.Plan?.PlanName ?? "Plus" });
                }

                return Ok(new { status = "none" });
            }

            var payOSSection = _configuration.GetSection("PayOS");
            var clientId = payOSSection["ClientId"] ?? string.Empty;
            var apiKey = payOSSection["ApiKey"] ?? string.Empty;
            var checksumKey = payOSSection["ChecksumKey"] ?? string.Empty;

            try
            {
                var payOS = new PayOSClient(clientId, apiKey, checksumKey);
                long orderCode = long.Parse(pendingPayment.TransactionId ?? "0");

                var paymentInfo = await payOS.PaymentRequests.GetAsync(orderCode);

                if (paymentInfo.Status.ToString() == "PAID")
                {
                    pendingPayment.Status = "success";
                    pendingPayment.PaymentDate = DateTime.UtcNow;
                    pendingPayment.UpdatedAt = DateTime.UtcNow;

                    pendingPayment.UserSub.Status = "active";
                    pendingPayment.UserSub.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    int duration = pendingPayment.UserSub.Plan?.DurationDays ?? 30;
                    pendingPayment.UserSub.EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(duration));
                    pendingPayment.UserSub.UpdatedAt = DateTime.UtcNow;

                    // Deactivate other active subscriptions for this user
                    var otherActiveSubs = await _context.UserSubscriptions
                        .Where(s => s.UserId == userId && s.UserSubId != pendingPayment.UserSubId && s.Status == "active")
                        .ToListAsync();

                    foreach (var sub in otherActiveSubs)
                    {
                        sub.Status = "inactive";
                        sub.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new { status = "success", plan = pendingPayment.UserSub.Plan?.PlanName ?? "Plus" });
                }
                else if (paymentInfo.Status.ToString() == "CANCELLED" || paymentInfo.Status.ToString() == "EXPIRED")
                {
                    pendingPayment.Status = "failed";
                    pendingPayment.UpdatedAt = DateTime.UtcNow;

                    pendingPayment.UserSub.Status = "cancelled";
                    pendingPayment.UserSub.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return Ok(new { status = "failed" });
                }

                return Ok(new { status = "pending" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error syncing with PayOS: " + ex.Message });
            }
        }

        [HttpPost("cancel")]
        [Authorize]
        public async Task<IActionResult> CancelPayment([FromQuery] Guid userId)
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || Guid.Parse(userIdClaim) != userId)
            {
                return Unauthorized("Unauthorized access to cancel payment.");
            }
            var pendingPayment = await _context.Payments
                .Include(p => p.UserSub)
                .Where(p => p.UserSub.UserId == userId && p.Status == "pending")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pendingPayment == null)
            {
                return BadRequest(new { message = "No pending payment found to cancel." });
            }

            var payOSSection = _configuration.GetSection("PayOS");
            var clientId = payOSSection["ClientId"] ?? string.Empty;
            var apiKey = payOSSection["ApiKey"] ?? string.Empty;
            var checksumKey = payOSSection["ChecksumKey"] ?? string.Empty;

            try
            {
                var payOS = new PayOSClient(clientId, apiKey, checksumKey);
                long orderCode = long.Parse(pendingPayment.TransactionId ?? "0");

                try
                {
                    await payOS.PaymentRequests.CancelAsync(orderCode, "User cancelled from App");
                }
                catch (Exception payOSEx)
                {
                    Console.WriteLine("PayOS cancel link error: " + payOSEx.Message);
                }

                pendingPayment.Status = "failed";
                pendingPayment.UpdatedAt = DateTime.UtcNow;

                pendingPayment.UserSub.Status = "cancelled";
                pendingPayment.UserSub.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Payment link cancelled successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error cancelling payment: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var now = DateTime.UtcNow;
            var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            
            var todayRevenue = await _context.Payments
                .Where(p => p.Status == "success" && p.PaymentDate >= startOfToday)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthRevenue = await _context.Payments
                .Where(p => p.Status == "success" && p.PaymentDate >= startOfMonth)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearRevenue = await _context.Payments
                .Where(p => p.Status == "success" && p.PaymentDate >= startOfYear)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            // Daily breakdown for the last 30 days
            var dailyRevenue = await _context.Payments
                .Where(p => p.Status == "success" && p.PaymentDate >= now.AddDays(-30))
                .GroupBy(p => p.PaymentDate!.Value.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(p => p.Amount) })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Monthly breakdown for this year
            var monthlyRevenue = await _context.Payments
                .Where(p => p.Status == "success" && p.PaymentDate >= startOfYear)
                .GroupBy(p => p.PaymentDate!.Value.Month)
                .Select(g => new { Month = g.Key, Amount = g.Sum(p => p.Amount) })
                .OrderBy(m => m.Month)
                .ToListAsync();

            return Ok(new {
                today = (double)todayRevenue,
                month = (double)monthRevenue,
                year = (double)yearRevenue,
                daily = dailyRevenue.Select(d => new { date = d.Date.ToString("yyyy-MM-dd"), amount = (double)d.Amount }),
                monthly = monthlyRevenue.Select(m => new { month = m.Month, amount = (double)m.Amount })
            });
        }

        [HttpGet("mock-checkout")]
        [AllowAnonymous]
        public async Task<IActionResult> MockCheckout(
            [FromQuery] Guid userId,
            [FromQuery] Guid planId,
            [FromQuery] string orderCode,
            [FromQuery] string? status = "success")
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            var user = await _context.Users.FindAsync(userId);

            if (plan == null || user == null)
            {
                return BadRequest("Invalid request parameters.");
            }

            bool isSuccess = status != "cancelled";

            // Process the transaction in the database
            var payment = await _context.Payments
                .Include(p => p.UserSub)
                .FirstOrDefaultAsync(p => p.TransactionId == orderCode);

            if (payment != null)
            {
                if (isSuccess)
                {
                    payment.Status = "success";
                    payment.PaymentDate = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;

                    payment.UserSub.Status = "active";
                    payment.UserSub.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    int duration = plan.DurationDays;
                    payment.UserSub.EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(duration));
                    payment.UserSub.UpdatedAt = DateTime.UtcNow;

                    // Deactivate other active subscriptions for this user
                    var otherActiveSubs = await _context.UserSubscriptions
                        .Where(s => s.UserId == userId && s.UserSubId != payment.UserSubId && s.Status == "active")
                        .ToListAsync();

                    foreach (var sub in otherActiveSubs)
                    {
                        sub.Status = "inactive";
                        sub.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    payment.Status = "failed";
                    payment.ErrorMessage = "Payment cancelled by user.";
                    payment.UpdatedAt = DateTime.UtcNow;

                    payment.UserSub.Status = "cancelled";
                    payment.UserSub.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            // Return a beautiful HTML confirmation page
            string pageTitle = isSuccess ? "Payment Successful!" : "Payment Failed";
            string iconColor = isSuccess ? "#10B981" : "#EF4444";
            string iconHtml = isSuccess 
                ? "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"2.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z\" /></svg>"
                : "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"2.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"m9.75 9.75 4.5 4.5m0-4.5-4.5 4.5M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z\" /></svg>";

            string description = isSuccess
                ? $"Your account has been successfully upgraded to the <strong>{plan.PlanName}</strong> plan.<br>Now you can experience all the unlimited features of SyncroLife!"
                : "The transaction was cancelled or encountered an error. Please try again.";

            string html = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{pageTitle}</title>
    <link href=""https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;600;700&display=swap"" rel=""stylesheet"">
    <style>
        body {{
            font-family: 'Plus Jakarta Sans', sans-serif;
            background-color: #0B0F19;
            color: #F3F4F6;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            padding: 20px;
            box-sizing: border-box;
        }}
        .card {{
            background: linear-gradient(135deg, #111827 0%, #1F2937 100%);
            border: 1px solid #374151;
            border-radius: 24px;
            padding: 40px 30px;
            width: 100%;
            max-width: 440px;
            text-align: center;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.3);
        }}
        .icon-wrapper {{
            width: 80px;
            height: 80px;
            background-color: rgba(255, 255, 255, 0.05);
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0 auto 24px auto;
            color: {iconColor};
        }}
        .icon-wrapper svg {{
            width: 48px;
            height: 48px;
        }}
        h1 {{
            font-size: 24px;
            margin-top: 0;
            margin-bottom: 12px;
            font-weight: 700;
        }}
        p {{
            font-size: 14px;
            color: #9CA3AF;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        .btn {{
            display: block;
            width: 100%;
            background-color: #3B82F6;
            color: white;
            text-decoration: none;
            padding: 14px 20px;
            border-radius: 12px;
            font-weight: 600;
            font-size: 14px;
            transition: background-color 0.2s;
            box-sizing: border-box;
        }}
        .btn:hover {{
            background-color: #2563EB;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon-wrapper"">
            {iconHtml}
        </div>
        <h1>{pageTitle}</h1>
        <p>{description}</p>
        <a href=""#"" onclick=""window.close(); return false;"" class=""btn"">Close Window</a>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }
    }

    public class CreatePaymentRequest
    {
        public Guid PlanId { get; set; }
    }
}
