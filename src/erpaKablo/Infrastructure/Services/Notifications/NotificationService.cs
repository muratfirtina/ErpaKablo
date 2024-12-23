using System.Net.Http.Json;
using Application.Abstraction.Services;
using Infrastructure.Services.Security.Models;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public NotificationService(
        ILogger<NotificationService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task SendEmailAsync(string subject, string message)
    {
        var emailSettings = _configuration.GetSection("Email").Get<EmailSettings>();
        
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("ErpaKablo Monitoring", emailSettings.FromAddress));
        email.To.Add(new MailboxAddress("Admin", emailSettings.ToAddress));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = message };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
        await client.SendAsync(email);
        await client.DisconnectAsync(true);
    }

    public async Task SendSlackMessageAsync(string message)
    {
        var slackWebhookUrl = _configuration["Slack:WebhookUrl"];
        if (string.IsNullOrEmpty(slackWebhookUrl))
        {
            _logger.LogWarning("Slack webhook URL is not configured");
            return;
        }

        var payload = new
        {
            text = message
        };

        var response = await _httpClient.PostAsJsonAsync(slackWebhookUrl, payload);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to send Slack notification. Status: {StatusCode}", 
                response.StatusCode);
        }
    }

    public Task SendTeamsMessageAsync(string message)
    {
        throw new NotImplementedException();
    }
}