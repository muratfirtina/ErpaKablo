using System.Text;
using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Application.Exceptions;
using Application.Features.Orders.Dtos;
using Domain;
using Ganss.Xss;
using Infrastructure.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Infrastructure.Services.Mail;

using MailKit.Security;
using MimeKit;
using Microsoft.Graph;

public class UnifiedMailService : IMailService
{
    private readonly ILogger<UnifiedMailService> _logger;
    private readonly IKeyVaultService _keyVaultService;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metricsService;
    private readonly SemaphoreSlim _throttler;
    private readonly EmailProvider _emailProvider;
    private IConfidentialClientApplication? _confidentialClientApp;

    public UnifiedMailService(
        ILogger<UnifiedMailService> logger,
        IKeyVaultService keyVaultService,
        IConfiguration configuration,
        ICacheService cacheService,
        IMetricsService metricsService)
    {
        _logger = logger;
        _keyVaultService = keyVaultService;
        _configuration = configuration;
        _cacheService = cacheService;
        _metricsService = metricsService;
        _throttler = new SemaphoreSlim(5, 5);
        _emailProvider = configuration.GetValue<EmailProvider>("Email:Provider");

        InitializeEmailClient();
    }

    private void InitializeEmailClient()
    {
        if (_emailProvider == EmailProvider.MicrosoftGraph)
        {
            _confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(_configuration["AzureAd:ClientId"])
                .WithClientSecret(_keyVaultService.GetSecretAsync("AzureAdClientSecret").Result)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"]}"))
                .Build();
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true)
    {
        await SendEmailAsync(new[] { to }, subject, body, isBodyHtml);
    }

    public async Task SendEmailAsync(string[] tos, string subject, string body, bool isBodyHtml = true)
    {
        try
        {
            // Rate limiting check
            await CheckRateLimit(tos);

            // Throttling
            if (!await _throttler.WaitAsync(TimeSpan.FromSeconds(30)))
                throw new ThrottlingException("Email sending is currently throttled");

            try
            {
                var sanitizer = new HtmlSanitizer();
                var sanitizedBody = sanitizer.Sanitize(body);

                switch (_emailProvider)
                {
                    case EmailProvider.MicrosoftGraph:
                        await SendViaGraphApiAsync(tos, subject, sanitizedBody, isBodyHtml);
                        break;
                    case EmailProvider.Google:
                        await SendViaGoogleAsync(tos, subject, sanitizedBody, isBodyHtml);
                        break;
                    case EmailProvider.Yandex:
                        await SendViaSmtpAsync(tos, subject, sanitizedBody, isBodyHtml, "smtp.yandex.com", 465);
                        break;
                    case EmailProvider.Custom:
                        var smtpServer = await _keyVaultService.GetSecretAsync("SmtpServer");
                        var smtpPort = int.Parse(await _keyVaultService.GetSecretAsync("SmtpPort"));
                        await SendViaSmtpAsync(tos, subject, sanitizedBody, isBodyHtml, smtpServer, smtpPort);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported email provider: {_emailProvider}");
                }

                _metricsService.IncrementTotalRequests("EMAIL", "SEND", "200");
            }
            finally
            {
                _throttler.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            _metricsService.IncrementTotalRequests("EMAIL", "SEND", "500");
            throw;
        }
    }

    private async Task SendViaSmtpAsync(string[] tos, string subject, string body, bool isBodyHtml, string server, int port)
    {
        var message = new MimeMessage();
        var username = await _keyVaultService.GetSecretAsync($"{_emailProvider}Username");
        var password = await _keyVaultService.GetSecretAsync($"{_emailProvider}Password");
        
        message.From.Add(new MailboxAddress(_configuration["Email:FromName"], username));
        message.To.AddRange(tos.Select(x => new MailboxAddress("", x)));
        message.Subject = subject;
        message.Body = new TextPart(isBodyHtml ? "html" : "plain") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(server, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private async Task SendViaGoogleAsync(string[] tos, string subject, string body, bool isBodyHtml)
    {
        // Google SMTP implementation
        await SendViaSmtpAsync(tos, subject, body, isBodyHtml, "smtp.gmail.com", 587);
    }

    private async Task SendViaGraphApiAsync(string[] tos, string subject, string body, bool isBodyHtml)
    {
        if (_confidentialClientApp == null)
            throw new InvalidOperationException("Graph API client not configured");

        var accessToken = await GetAccessTokenAsync();
        var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(request =>
        {
            request.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            return Task.CompletedTask;
        }));

        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = isBodyHtml ? BodyType.Html : BodyType.Text,
                Content = body
            },
            ToRecipients = tos.Select(to => new Recipient
            {
                EmailAddress = new EmailAddress { Address = to }
            }).ToList()
        };

        await graphClient.Users[await _keyVaultService.GetSecretAsync("GraphApiSenderAddress")]
            .SendMail(message, true).Request().PostAsync();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (_confidentialClientApp == null)
            throw new InvalidOperationException("Graph API client not configured");

        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var authResult = await _confidentialClientApp.AcquireTokenForClient(scopes).ExecuteAsync();
        return authResult.AccessToken;
    }

    private async Task CheckRateLimit(string[] recipients)
    {
        foreach (var recipient in recipients)
        {
            var rateLimitKey = $"email_ratelimit_{recipient}_{DateTime.UtcNow:yyyyMMddHH}";
            var count = await _cacheService.GetCounterAsync(rateLimitKey);
            
            if (count >= 10)
                throw new RateLimitExceededException($"Email rate limit exceeded for recipient: {recipient}");
                
            await _cacheService.IncrementAsync(rateLimitKey, 1, TimeSpan.FromHours(1));
        }
    }

    private async Task<string> BuildEmailTemplate(string content, string title = "")
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        </head>
        <body style='margin: 0; padding: 0; background-color: #f6f9fc; font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px;'>
                <div style='text-align: center; padding: 20px; background-color: #004d99; margin-bottom: 20px;'>
                    <img src='{await _keyVaultService.GetSecretAsync("CompanyLogo")}' alt='ErpaKablo Logo' style='max-width: 200px;'/>
                </div>

                {(string.IsNullOrEmpty(title) ? "" : $"<h1 style='color: #004d99; text-align: center; margin-bottom: 30px;'>{title}</h1>")}

                <div style='padding: 20px; line-height: 1.6;'>
                    {content}
                </div>

                <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; text-align: center;'>
                    {await BuildFooterContent()}
                </div>
            </div>
        </body>
        </html>";
    }

    private async Task<string> BuildFooterContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"<p style='color: #666; font-size: 12px;'>
            Bu bir otomatik bilgilendirme mesajıdır.<br>
            Herhangi bir sorunuz varsa bizimle iletişime geçebilirsiniz.
        </p>");

        // Social media links
        var socialLinks = new[]
        {
            ("Facebook", await _keyVaultService.GetSecretAsync("SocialMedia:Facebook")),
            ("Twitter", await _keyVaultService.GetSecretAsync("SocialMedia:Twitter")),
            ("LinkedIn", await _keyVaultService.GetSecretAsync("SocialMedia:LinkedIn"))
        };

        sb.AppendLine("<div style='margin-top: 20px;'>");
        foreach (var (platform, url) in socialLinks)
        {
            sb.AppendLine($@"
                <a href='{url}' style='margin: 0 10px; text-decoration: none;'>
                    <img src='{await _keyVaultService.GetSecretAsync($"SocialMedia:{platform}Icon")}' 
                         alt='{platform}' style='width: 24px;'/>
                </a>");
        }
        sb.AppendLine("</div>");

        // Company info
        var companyInfo = new StringBuilder();
        companyInfo.AppendLine("<div style='margin-top: 20px; color: #666; font-size: 12px;'>");
        companyInfo.AppendLine("<p>ErpaKablo A.Ş.</p>");
        companyInfo.AppendLine($"<p>{await _keyVaultService.GetSecretAsync("CompanyAddress")}</p>");
        companyInfo.AppendLine($"<p>Tel: {await _keyVaultService.GetSecretAsync("CompanyPhone")} | " +
                              $"Email: {await _keyVaultService.GetSecretAsync("CompanyEmail")}</p>");
        companyInfo.AppendLine($"<p>&copy; {DateTime.Now.Year} ErpaKablo. Tüm hakları saklıdır.</p>");
        companyInfo.AppendLine("</div>");

        sb.Append(companyInfo);
        return sb.ToString();
    }

    public async Task SendPasswordResetEmailAsync(string to, string userId, string resetToken)
    {
        var resetLink = $"{await _keyVaultService.GetSecretAsync("AngularClientUrl")}/password-update/{userId}/{resetToken}";
        var content = $@"
            <div style='text-align: center;'>
                <img src='{await _keyVaultService.GetSecretAsync("Icons:PasswordReset")}' 
                     alt='Password Reset' style='width: 100px; margin-bottom: 20px;'/>
                <p style='font-size: 16px; color: #333;'>Merhaba,</p>
                <p style='font-size: 16px; color: #333;'>Şifre sıfırlama talebiniz alınmıştır.</p>
                <div style='margin: 30px 0;'>
                    <a href='{resetLink}' style='background-color: #004d99; color: white; 
                              padding: 12px 30px; text-decoration: none; border-radius: 5px;
                              font-weight: bold; display: inline-block;'>
                        Şifremi Yenile
                    </a>
                </div>
                <p style='color: #666; font-size: 14px;'>
                    Bu işlemi siz başlatmadıysanız, lütfen bu e-postayı dikkate almayınız.
                </p>
                <p style='color: #666; font-size: 14px;'>
                    Güvenliğiniz için şifre sıfırlama bağlantısı 1 saat süreyle geçerlidir.
                </p>
            </div>";

        await SendEmailAsync(to, "Şifre Sıfırlama İsteği", 
            await BuildEmailTemplate(content, "Şifre Sıfırlama"));
    }

    public async Task SendCompletedOrderEmailAsync(
        string to, string orderCode, string orderDescription,
        UserAddress orderAddress, DateTime orderCreatedDate, string userName, 
        List<OrderItemDto> orderCartItems, decimal? orderTotalPrice)
    {
        var content = new StringBuilder();
        content.Append(await BuildOrderConfirmationContent(
            userName, orderCode, orderDescription, orderAddress, 
            orderCreatedDate, orderCartItems, orderTotalPrice));

        await SendEmailAsync(to, "Siparişiniz Tamamlandı ✓", 
            await BuildEmailTemplate(content.ToString(), "Sipariş Onayı"));
    }

    public async Task SendOrderUpdateNotificationAsync(
        string to, string orderCode, string adminNote,
        List<OrderItem> updatedItems, decimal? totalPrice)
    {
        var content = new StringBuilder();
        content.Append(await BuildOrderUpdateContent(
            orderCode, adminNote, updatedItems, totalPrice));

        await SendEmailAsync(to, "Sipariş Güncelleme Bildirimi", 
            await BuildEmailTemplate(content.ToString(), "Sipariş Güncelleme"));
    }

    private async Task<string> BuildOrderConfirmationContent(
        string userName, string orderCode, string orderDescription,
        UserAddress orderAddress, DateTime orderCreatedDate,
        List<OrderItemDto> orderCartItems, decimal? orderTotalPrice)
    {
        var storageUrl = await _keyVaultService.GetSecretAsync("Storage:Providers:LocalStorage:Url");
        var sb = new StringBuilder();

        // Header
        sb.Append($@"
            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
                <p style='font-size: 16px; color: #333;'>Merhaba {userName},</p>
                <p style='color: #666;'>Siparişiniz başarıyla oluşturulmuştur.</p>
            </div>");

        // Order details table
        sb.Append(await BuildOrderItemsTable(orderCartItems, storageUrl));

        // Order information
        sb.Append($@"
            <div style='margin-top: 30px; padding: 20px; background-color: #f8f9fa; border-radius: 5px;'>
                <h3 style='color: #004d99; margin-bottom: 15px;'>Sipariş Bilgileri</h3>
                <p><strong>Sipariş Kodu:</strong> {orderCode}</p>
                <p><strong>Sipariş Tarihi:</strong> {orderCreatedDate:dd.MM.yyyy HH:mm}</p>
                <p><strong>Teslimat Adresi:</strong><br>{orderAddress}</p>
                <p><strong>Sipariş Notu:</strong><br>{orderDescription}</p>
            </div>");

        return sb.ToString();
    }

    private async Task<string> BuildOrderUpdateContent(
        string orderCode, string adminNote,
        List<OrderItem> updatedItems, decimal? totalPrice)
    {
        var sb = new StringBuilder();

        // Header
        sb.Append($@"
            <div style='background-color: #fff3cd; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
                <p style='color: #856404;'>Siparişinizde güncelleme yapılmıştır.</p>
                <p style='color: #856404;'><strong>Sipariş Kodu:</strong> {orderCode}</p>
                <p style='color: #856404;'><strong>Admin Notu:</strong> {adminNote}</p>
            </div>");

        // Updated items table
        sb.Append(await BuildUpdatedItemsTable(updatedItems, totalPrice));

        return sb.ToString();
    }

    private async Task<string> BuildOrderItemsTable(List<OrderItemDto> items, string storageUrl)
    {
        var sb = new StringBuilder();
        sb.Append(@"
            <table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>
                <tr style='background-color: #004d99; color: white;'>
                    <th style='padding: 12px; text-align: left;'>Ürün</th>
                    <th style='padding: 12px; text-align: right;'>Fiyat</th>
                    <th style='padding: 12px; text-align: center;'>Adet</th>
                    <th style='padding: 12px; text-align: right;'>Toplam</th>
                    <th style='padding: 12px; text-align: center;'>Görsel</th>
                </tr>");

        foreach (var item in items)
        {
            sb.Append($@"
                <tr style='border-bottom: 1px solid #e0e0e0;'>
                    <td style='padding: 12px;'>
                        <strong style='color: #333;'>{item.BrandName}</strong><br>
                        <span style='color: #666;'>{item.ProductName}</span>
                    </td>
                    <td style='padding: 12px; text-align: right;'>{item.Price:C2}</td>
                    <td style='padding: 12px; text-align: center;'>{item.Quantity}</td>
                    <td style='padding: 12px; text-align: right;'>{(item.Price * item.Quantity):C2}</td>
                    <td style='padding: 12px; text-align: center;'>
                        <img src='{storageUrl}/{item.ShowcaseImage?.EntityType}/{item.ShowcaseImage?.Path}/{item.ShowcaseImage?.FileName}'
                             style='max-width: 80px; max-height: 80px; border-radius: 4px;'
                             alt='{item.ProductName}'/>
                    </td>
                </tr>");
        }

        sb.Append(@"</table>");
        return sb.ToString();
    }

    private async Task<string> BuildUpdatedItemsTable(List<OrderItem> items, decimal? totalPrice)
    {
        var sb = new StringBuilder();
        sb.Append(@"
            <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                <tr style='background-color: #004d99; color: white;'>
                    <th style='padding: 12px; text-align: left;'>Ürün</th>
                    <th style='padding: 12px; text-align: right;'>Eski Fiyat</th>
                    <th style='padding: 12px; text-align: right;'>Yeni Fiyat</th>
                    <th style='padding: 12px; text-align: center;'>Termin Süresi</th>
                </tr>");

        foreach (var item in items)
        {
            var priceChange = item.UpdatedPrice > item.Price ? "color: #dc3545;" : "color: #28a745;";
            sb.Append($@"
                <tr style='border-bottom: 1px solid #e0e0e0;'>
                    <td style='padding: 12px;'>{item.ProductName}</td>
                    <td style='padding: 12px; text-align: right;'>{item.Price:C2}</td>
                    <td style='padding: 12px; text-align: right; {priceChange}'>{item.UpdatedPrice:C2}</td>
                    <td style='padding: 12px; text-align: center;'>{item.LeadTime} gün</td>
                </tr>");
        }

        if (totalPrice.HasValue)
        {
            sb.Append($@"
                <tr style='background-color: #f8f9fa;'>
                    <td colspan='2' style='padding: 12px; text-align: right;'><strong>Güncel Toplam Tutar:</strong></td>
                    <td colspan='2' style='padding: 12px; text-align: right;'><strong>{totalPrice:C2}</strong></td>
                </tr>");
        }

        sb.Append("</table>");
        return sb.ToString();
    }
}
