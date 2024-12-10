using System.Text;
using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Application.Exceptions;
using Application.Extensions;
using Application.Features.Orders.Dtos;
using Application.Features.UserAddresses.Dtos;
using Application.Storage;
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
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly IMetricsService _metricsService;
    private readonly SemaphoreSlim _throttler;
    private readonly EmailProvider _emailProvider;
    private readonly IStorageService _storageService;
    private IConfidentialClientApplication? _confidentialClientApp;
    private static string? _cachedLogoUrl;

    public UnifiedMailService(
        ILogger<UnifiedMailService> logger,
        IConfiguration configuration,
        ICacheService cacheService,
        IMetricsService metricsService, IStorageService storageService)
    {
        _logger = logger;
        _configuration = configuration;
        _cacheService = cacheService;
        _metricsService = metricsService;
        _storageService = storageService;
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
                .WithClientSecret(_configuration["AzureAd:ClientSecret"])
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
                        var smtpServer = _configuration["Smtp:Server"];
                        var smtpPort = int.Parse(_configuration["Smtp:Port"]);
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
        
        string username = _configuration[$"Email:Providers:{_emailProvider}:Username"];
        string password = _configuration[$"Email:Providers:{_emailProvider}:Password"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Email credentials not configured");
        }
    
        message.From.Add(new MailboxAddress(_configuration["Email:FromName"], username));
        message.To.AddRange(tos.Select(x => new MailboxAddress("", x)));
        message.Subject = subject;
        message.Body = new TextPart(isBodyHtml ? "html" : "plain") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        try 
        {
            await client.ConnectAsync(server, port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
        
            _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", tos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", tos));
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private async Task SendViaGoogleAsync(string[] tos, string subject, string body, bool isBodyHtml)
    {
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

        await graphClient.Users[_configuration["Graph:SenderAddress"]]
            .SendMail(message, true)
            .Request()
            .PostAsync();
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

    public async Task<string> BuildEmailTemplate(string content, string title = "")
    {
        var logoUrl = _storageService.GetCompanyLogoUrl();
        
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        </head>
        <body style='margin: 0; padding: 0; background-color: #f6f9fc; font-family: Arial, sans-serif;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px;'>
                <div style='text-align: center; padding: 20px; background-color: #e0e0e0; margin-bottom: 20px;'>
                    <img src='{logoUrl}' alt='Company Logo' style='max-width: 200px;'/>
                </div>
                
                {(string.IsNullOrEmpty(title) ? "" : $"<h1 style='color: #059669; text-align: center; margin-bottom: 30px;'>{title}</h1>")}

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
        var companyInfo = _configuration.GetSection("CompanyInfo");
        var socialMedia = companyInfo.GetSection("SocialMedia");
        var cloudinaryUrl = _configuration["Storage:Providers:Cloudinary:Url"]?.TrimEnd('/');

        var sb = new StringBuilder();
        sb.AppendLine(@"<p style='color: #666; font-size: 12px;'>
        This is an automated notification message.<br>
        If you have any questions, please contact us.
    </p>");

        // Social media links - Cloudinary'den ikon URL'lerini al
        var socialLinks = new[]
        {
            ("Facebook", socialMedia["Facebook"], $"{cloudinaryUrl}/social-media/icons/facebook.png"),
            ("Twitter", socialMedia["Twitter"], $"{cloudinaryUrl}/social-media/icons/twitter.png"),
            ("LinkedIn", socialMedia["LinkedIn"], $"{cloudinaryUrl}/social-media/icons/linkedin.png")
        };

        sb.AppendLine("<div style='margin-top: 20px;'>");
        foreach (var (platform, url, iconUrl) in socialLinks)
        {
            if (!string.IsNullOrEmpty(url))
            {
                sb.AppendLine($@"
                <a href='{url}' style='margin: 0 10px; text-decoration: none;'>
                    <img src='{iconUrl}' alt='{platform}' style='width: 24px;'/>
                </a>");
            }
        }
        sb.AppendLine("</div>");

        // Company info
        var companyInfoBuilder = new StringBuilder();
        companyInfoBuilder.AppendLine("<div style='margin-top: 20px; color: #666; font-size: 12px;'>");
        companyInfoBuilder.AppendLine($"<p>{companyInfo["Name"]}</p>");
        companyInfoBuilder.AppendLine($"<p>{companyInfo["Address"]}</p>");
        companyInfoBuilder.AppendLine($"<p>Tel: {companyInfo["Phone"]} | Email: {companyInfo["Email"]}</p>");
        companyInfoBuilder.AppendLine($"<p>&copy; {DateTime.Now.Year} {companyInfo["Name"]}. All rights reserved.</p>");
        companyInfoBuilder.AppendLine("</div>");

        sb.Append(companyInfoBuilder);
        return sb.ToString();
    }

    public async Task SendPasswordResetEmailAsync(string to, string userId, string resetToken)
    {
        var clientUrl = _configuration["AngularClientUrl"] ?? "http://localhost:4200";
        var resetLink = $"{clientUrl.TrimEnd('/')}/update-password/{userId}/{resetToken}";
        
        var content = $@"
            <div style='text-align: center;'>
                <p style='font-size: 16px; color: #333;'>Dear User,</p>
                <p style='font-size: 16px; color: #333;'>We have received your password reset request.</p>
                <div style='margin: 30px 0;'>
                    <a href='{resetLink}' style='background-color: #e53935; color: white; 
                              padding: 12px 30px; text-decoration: none; border-radius: 5px;
                              font-weight: bold; display: inline-block;'>
                        Reset Password
                    </a>
                </div>
                <p style='color: #666; font-size: 14px;'>
                    If you didn't initiate this request, please ignore this email.
                </p>
                <p style='color: #666; font-size: 14px;'>
                    For your security, this password reset link is valid for 1 hour.
                </p>
            </div>";

        var emailSubject = "Password Reset Request";
        
        var emailBody = $@"
            <!DOCTYPE html>
            <html>
            <body style='margin: 0; padding: 20px; background-color: #f6f9fc; font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 5px;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h1 style='color: #e53935;'>Password Reset</h1>
                    </div>
                    {content}
                    <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; text-align: center;'>
                        <p style='color: #666; font-size: 12px;'>
                            © {DateTime.Now.Year} Company Name. All rights reserved.
                        </p>
                    </div>
                </div>
            </body>
            </html>";

        await SendEmailAsync(to, emailSubject, emailBody);
    }

    public async Task SendCreatedOrderEmailAsync(
        string to, 
        string orderCode, 
        string orderDescription,
        UserAddressDto? orderAddress, 
        DateTime orderCreatedDate, 
        string userName,
        List<OrderItemDto> orderCartItems, 
        decimal? orderTotalPrice)
    {
        try 
        {
            var content = new StringBuilder();
            
            // NOT: Artık burada herhangi bir image URL dönüşümü yapmıyoruz
            // Çünkü ConvertCartToOrder'da ToDto() ile dönüşüm yapılmış olmalı

            content.Append(await BuildOrderConfirmationContent(
                userName, 
                orderCode, 
                orderDescription, 
                orderAddress,
                orderCreatedDate, 
                orderCartItems, 
                orderTotalPrice));

            var emailBody = await BuildEmailTemplate(content.ToString(), "Order Confirmation");
            await SendEmailAsync(to, "Order Created ✓", emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email");
            throw;
        }
    }

    public async Task SendOrderUpdateNotificationAsync(
        string to, string orderCode, string adminNote,
        List<OrderItem> updatedItems, decimal? totalPrice)
    {
        var content = new StringBuilder();
        content.Append(await BuildOrderUpdateContent(
            orderCode, adminNote, updatedItems, totalPrice));

        await SendEmailAsync(to, "Order Update Notification", 
            await BuildEmailTemplate(content.ToString(), "Order Update"));
    }

    private async Task<string> BuildOrderConfirmationContent(
        string userName, string orderCode, string orderDescription,
        UserAddressDto? orderAddress, DateTime orderCreatedDate,
        List<OrderItemDto> orderCartItems, decimal? orderTotalPrice)
    {
        var storageUrl = _configuration["Storage:Providers:Cloudinary:Url"] ?? 
                         _configuration["Storage:Providers:LocalStorage:Url"];
        var sb = new StringBuilder();

        // Header
        sb.Append($@"
        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
            <p style='font-size: 16px; color: #333;'>Hello {userName},</p>
            <p style='color: #666;'>Your order has been successfully created.</p>
        </div>");

        // Order details table
        sb.Append(BuildOrderItemsTable(orderCartItems, storageUrl));

        // Order information
        sb.Append($@"
        <div style='margin-top: 30px; padding: 20px; background-color: #f8f9fa; border-radius: 5px;'>
            <h3 style='color: #333333; margin-bottom: 15px;'>Order Details</h3>
            <p style='color: #e53935;'><strong>Order Code:{orderCode}</strong></p>
            <p><strong>Order Date:</strong> {orderCreatedDate:dd.MM.yyyy HH:mm}</p>
            <p><strong>Delivery Address:</strong><br>{FormatAddress(orderAddress)}</p>
            <p><strong>Order Note:</strong><br>{orderDescription}</p>
            <p style='color: #059669;'><strong>Total Amount:</strong><br>₺{orderTotalPrice:N2}</p>
        </div>");

        return sb.ToString();
    }
    private string FormatAddress(UserAddressDto? address)
    {
        if (address == null) return "No address provided";
    
        var formattedAddress = new StringBuilder();
        formattedAddress.AppendLine(address.Name);
        formattedAddress.AppendLine(address.AddressLine1);
    
        if (!string.IsNullOrEmpty(address.AddressLine2))
            formattedAddress.AppendLine(address.AddressLine2);
    
        formattedAddress.AppendLine($"{address.City}{(!string.IsNullOrEmpty(address.State) ? $", {address.State}" : "")} {address.PostalCode}");
        formattedAddress.Append(address.Country);
    
        return formattedAddress.ToString();
    }

    private async Task<string> BuildOrderUpdateContent(
        string orderCode, string adminNote,
        List<OrderItem> updatedItems, decimal? totalPrice)
    {
        var sb = new StringBuilder();

        // Header
        sb.Append($@"
            <div style='background-color: #fff3cd; padding: 20px; border-radius: 5px; margin-bottom: 20px;'>
                <p style='color: #856404;'>Your order has been updated.</p>
                <p style='color: #e53935;'><strong>Order Code:{orderCode}</strong></p>
                <p style='color: #856404;'><strong>Admin Note:</strong> {adminNote}</p>
            </div>");

        // Updated items table
        sb.Append(await BuildUpdatedItemsTable(updatedItems, totalPrice));

        return sb.ToString();
    }

    private string BuildOrderItemsTable(List<OrderItemDto> items, string? storageUrl)
    {
        var sb = new StringBuilder();
        decimal totalAmount = 0;
        
        sb.Append(@"
            <table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>
                <tr style='background-color: #333333; color: white;'>
                    <th style='padding: 12px; text-align: left;'>Product</th>
                    <th style='padding: 12px; text-align: right;'>Price</th>
                    <th style='padding: 12px; text-align: center;'>Quantity</th>
                    <th style='padding: 12px; text-align: right;'>Total</th>
                    <th style='padding: 12px; text-align: center;'>Image</th>
                </tr>");

        foreach (var item in items)
        {
            var itemTotal = (item.Price ?? 0) * (item.Quantity ?? 0);
            totalAmount += itemTotal;

            // ShowcaseImage zaten DTO formatında ve URL'i ConvertCartToOrder sırasında oluşturulmuş durumda
            string imageUrl = item.ShowcaseImage?.Url ?? 
                            _configuration["CompanyInfo:DefaultProductImage"] ?? "";

            sb.Append($@"
                <tr style='border-bottom: 1px solid #e0e0e0;'>
                    <td style='padding: 12px;'>
                        <strong style='color: #333;'>{item.BrandName}</strong><br>
                        <span style='color: #666;'>{item.ProductName}</span>
                    </td>
                    <td style='padding: 12px; text-align: right;'>₺{item.Price:N2}</td>
                    <td style='padding: 12px; text-align: center;'>{item.Quantity}</td>
                    <td style='padding: 12px; text-align: right;'>₺{itemTotal:N2}</td>
                    <td style='padding: 12px; text-align: center;'>
                        <img src='{imageUrl}'
                             style='max-width: 80px; max-height: 80px; border-radius: 4px;'
                             alt='{item.ProductName}'/>
                    </td>
                </tr>");
        }

        sb.Append($@"
            <tr style='background-color: #f8f9fa; color: #059669; font-weight: bold;'>
                <td colspan='3' style='padding: 12px; text-align: right;'>Total Amount:</td>
                <td colspan='2' style='padding: 12px; text-align: right;'>₺{totalAmount:N2}</td>
            </tr>");

        sb.Append("</table>");
        return sb.ToString();
    }


    private async Task<string> BuildUpdatedItemsTable(List<OrderItem> items, decimal? totalPrice)
    {
        var sb = new StringBuilder();
        sb.Append(@"
            <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                <tr style='background-color: #333333; color: white;'>
                    <th style='padding: 12px; text-align: left;'>Product</th>
                    <th style='padding: 12px; text-align: right;'>Old Price</th>
                    <th style='padding: 12px; text-align: right;'>New Price</th>
                    <th style='padding: 12px; text-align: center;'>Lead Time</th>
                </tr>");

        foreach (var item in items)
        {
            var priceChange = item.UpdatedPrice > item.Price ? "color: #dc3545;" : "color: #28a745;";
            sb.Append($@"
                <tr style='border-bottom: 1px solid #e0e0e0;'>
                    <td style='padding: 12px;'>{item.ProductName}</td>
                    <td style='padding: 12px; text-align: right;'>{item.Price:C2}</td>
                    <td style='padding: 12px; text-align: right; {priceChange}'>{item.UpdatedPrice:C2}</td>
                    <td style='padding: 12px; text-align: center;'>{item.LeadTime} days</td>
                </tr>");
        }

        if (totalPrice.HasValue)
        {
            sb.Append($@"
                <tr style='background-color: #f8f9fa; color: #059669; font-weight: bold;'>
                    <td colspan='2' style='padding: 12px; text-align: right;'><strong>Updated Total Amount:</strong></td>
                    <td colspan='2' style='padding: 12px; text-align: right;'><strong>{totalPrice:C2}</strong></td>
                </tr>");
        }

        sb.Append("</table>");
        return sb.ToString();
    }
    
}