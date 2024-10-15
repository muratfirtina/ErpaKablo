using System.Net;
using System.Net.Mail;
using System.Text;
using Application.Abstraction.Services;
using Application.Features.Orders.Dtos;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class MailService : IMailService
{
    readonly IConfiguration _configuration;

    public MailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true)
    {
        await SendEmailAsync(new []{to}, subject, body, isBodyHtml);
        
    }

    public async Task SendEmailAsync(string[] tos, string subject, string body, bool isBodyHtml = true)
    {
        MailMessage mail = new();
        mail.IsBodyHtml = isBodyHtml;
        foreach (var to in tos)
        {
            mail.To.Add(to);
        }
        mail.Subject = subject;
        mail.Body = body;
        mail.From = new MailAddress(_configuration["Mail:Username"] , "Mini-ETicaret", Encoding.UTF8);
        
        SmtpClient smtpClient = new();
        smtpClient.Credentials = new NetworkCredential(_configuration["Mail:Username"], _configuration["Mail:Password"]);
        smtpClient.Host = "smtp.office365.com"; // _configuration["Mail:Host"];
        smtpClient.Port = 587; //int.Parse(_configuration["Mail:Port"]);
        smtpClient.EnableSsl = true;
        await smtpClient.SendMailAsync(mail);
        
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
    public Task SendCompletedOrderEmailAsync(string to, string orderCode, string orderDescription, string orderAddress, DateTime orderCreatedDate, string userName, List<OrderCartItemDto> orderCartItems, float orderTotalPrice)
    {
        string subject = "Siparişiniz Tamamlandı";
        string body = $"Merhaba {userName},<br><br>";
    
        body += "<table style=\"border-collapse: collapse;\">";
        body += "<tr><th style=\"border: 1px solid black; padding: 8px;\">Ürün</th><th style=\"border: 1px solid black; padding: 8px;\">Fiyat</th><th style=\"border: 1px solid black; padding: 8px;\">Adet</th><th style=\"border: 1px solid black; padding: 8px;\">Toplam Fiyat</th><th style=\"border: 1px solid black; padding: 8px;\">Resimler</th></tr>";

        foreach (var item in orderCartItems)
        {
            body += "<tr>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.Name}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.Price}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.Quantity}</td>";
            body += $"<td style=\"border: 1px solid black; padding: 8px;\">{item.TotalPrice}</td>";
            body += "<td style=\"border: 1px solid black; padding: 8px;\">";

            foreach (var imageFile in item.ProductImageFiles)
            {
                body += $"<img src=\"{_configuration["BaseStorageUrl"]}/{imageFile.Path}\" style=\"max-width: 100px; max-height: 100px;\"><br>";
            }

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

    
}