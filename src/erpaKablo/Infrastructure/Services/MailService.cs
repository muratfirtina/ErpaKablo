using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using Application.Abstraction.Services;
using Application.Features.Orders.Dtos;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Infrastructure.Services;

public class MailService : IMailService
{
    readonly IConfiguration _configuration;
    private readonly IConfidentialClientApplication _confidentialClientApp;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _tenantId = _configuration["AzureAd:TenantId"];
            _clientId = _configuration["AzureAd:ClientId"];
            _clientSecret = _configuration["AzureAd:ClientSecret"];

            _confidentialClientApp = ConfidentialClientApplicationBuilder.Create(_clientId)
                .WithClientSecret(_clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_tenantId}"))
                .Build();
        }

        // OAuth 2.0 üzerinden AccessToken alır
        private async Task<string> GetAccessTokenAsync()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var authResult = await _confidentialClientApp.AcquireTokenForClient(scopes).ExecuteAsync();
            return authResult.AccessToken;
        }

        // Microsoft Graph API kullanarak mail gönderir
        public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true)
        {
            await SendEmailAsync(new[] { to }, subject, body, isBodyHtml);
        }

        // Microsoft Graph API kullanarak birden fazla alıcıya mail gönderir
        public async Task SendEmailAsync(string[] tos, string subject, string body, bool isBodyHtml = true)
        {
            // OAuth 2.0 token al
            var accessToken = await GetAccessTokenAsync();

            // Graph API istemcisi oluştur
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return Task.CompletedTask;
            }));

            // Mail içeriği oluştur
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
                    EmailAddress = new EmailAddress
                    {
                        Address = to
                    }
                }).ToList()
            };

            // Mail gönder
            await graphClient.Users["muratfirtina@hotmail.com"].SendMail(message, true).Request().PostAsync();
        }


    public async Task SendPasswordResetEmailAsync(string to, string userId, string resetToken)
    {
        string subject = "Şifre Sıfırlama İsteği";
        string resetLink = _configuration["AngularClientUrl"]+$"/password-update/{userId}/{resetToken}\n\n";

        string body = $"Merhaba,<br><br>Bu e-posta şifre sıfırlama talebinize istinaden gönderilmiştir. ";
        body += $"Aşağıdaki linke tıklayarak şifre yenileme sayfasına yönlendirileceksiniz:<br><br>";
        body += $"<a href='{resetLink}'>Yeni şifre talebi için tıklayınız</a><br><br>";
        body += "Eğer şifre sıfırlama talebi göndermediyseniz, bu e-postayı dikkate almayabilirsiniz.<br>";
        body += "İyi günler dileriz.";
            await SendEmailAsync(to, subject, body);
    }
    public Task SendCompletedOrderEmailAsync(string to, string orderCode, string orderDescription,
        UserAddress orderAddress, DateTime orderCreatedDate, string userName, List<OrderItemDto> orderCartItems,
        decimal? orderTotalPrice)
    {
        string subject = "Siparişiniz Tamamlandı";
        string body = $"Merhaba {userName},<br><br>";
    
        body += "<table style=\"border-collapse: collapse;\">";
        body += "<tr><th style=\"border: 1px solid black; padding: 8px;\">Ürün</th><th style=\"border: 1px solid black; padding: 8px;\">Fiyat</th><th style=\"border: 1px solid black; padding: 8px;\">Adet</th><th style=\"border: 1px solid black; padding: 8px;\">Toplam Fiyat</th><th style=\"border: 1px solid black; padding: 8px;\">Resimler</th></tr>";

        foreach (var item in orderCartItems)
        {
            body += "<tr>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.BrandName}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.ProductName}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.Price}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.Quantity}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.Price}</td>";
            body += "<td style=\"border: 1px solid black; padding: 8px;\">";
            body += $"<img src=\"{_configuration["Storage:Providers:LocalStorage:Url"]}/{item.ShowcaseImage?.EntityType}/{item.ShowcaseImage?.Path}/{item.ShowcaseImage?.FileName}\" style=\"max-width: 100px; max-height: 100px;\"><br>";


            body += "</td>";
            body += "</tr>";
        }

        body += "</table><br>";


        
        body += $"Siparişinizin Toplam Fiyatı: {orderTotalPrice}<br><br>";
        body += $"Siparişiniz {orderCreatedDate} tarihinde alınmıştır.<br><br>";
        body += $"Sipariş kodunuz: {orderCode}<br><br>";
        body += $"Siparişinizin teslim edileceği adres: {orderAddress}<br><br>";
        body += $"Siparişinizin açıklaması: {orderDescription}<br><br>";
        body += "İyi günler dileriz.";
    
        return SendEmailAsync(to, subject, body);
    }

    public async Task SendOrderUpdateNotificationAsync(
        string to, 
        string orderCode, 
        string adminNote,
        List<OrderItem> updatedItems,
        decimal? totalPrice)
    {
        string subject = "Siparişinizde Güncelleme";
        
        StringBuilder body = new StringBuilder();
        body.AppendLine($"<h2>Sipariş Güncelleme Bildirimi</h2>");
        body.AppendLine($"<p>Sipariş Kodu: {orderCode}</p>");
        body.AppendLine($"<p>Admin Notu: {adminNote}</p>");
        
        body.AppendLine("<h3>Güncellenen Ürünler:</h3>");
        body.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");
        body.AppendLine("<tr style='background-color: #f2f2f2;'>");
        body.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Ürün</th>");
        body.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Eski Fiyat</th>");
        body.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Yeni Fiyat</th>");
        body.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Termin Süresi</th>");
        body.AppendLine("</tr>");

        foreach (var item in updatedItems)
        {
            body.AppendLine("<tr>");
            body.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{item.ProductName}</td>");
            body.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{item.Price:C2}</td>");
            body.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{item.UpdatedPrice:C2}</td>");
            body.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{item.LeadTime} gün</td>");
            body.AppendLine("</tr>");
        }
        
        body.AppendLine("</table>");
        
        await SendEmailAsync(to, subject, body.ToString(), true);
    }
    
}