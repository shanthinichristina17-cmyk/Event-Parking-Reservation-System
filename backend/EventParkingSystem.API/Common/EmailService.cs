using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventParkingSystem.API.Common;

public sealed class EmailSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Event Park";
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";
}

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string name, string rawToken);
    Task SendPasswordResetAsync(string email, string name, string rawToken);
    Task SendBookingConfirmationAsync(string email, string name, string bookingNumber, decimal totalAmount);
}

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string email, string name, string rawToken)
    {
        var link = $"{_settings.FrontendBaseUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(rawToken)}";
        return SendAsync(email, name, "Verify your Event Park email",
            $"Hello {name},\n\nVerify your email using this link:\n{link}\n\nThis link expires in 24 hours.");
    }

    public Task SendPasswordResetAsync(string email, string name, string rawToken)
    {
        var link = $"{_settings.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        return SendAsync(email, name, "Reset your Event Park password",
            $"Hello {name},\n\nReset your password using this link:\n{link}\n\nThis link expires in 45 minutes.");
    }

    public Task SendBookingConfirmationAsync(string email, string name, string bookingNumber, decimal totalAmount)
        => SendAsync(email, name, $"Booking confirmed - {bookingNumber}",
            $"Hello {name},\n\nYour booking {bookingNumber} is confirmed. Total paid: LKR {totalAmount:N2}.\n\nThank you.");

    private async Task SendAsync(string email, string name, string subject, string body)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("EMAIL DISABLED. To={Email}; Subject={Subject}; Body={Body}", email, subject, body);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(name, email));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        var socket = _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        await client.ConnectAsync(_settings.Host, _settings.Port, socket);
        if (!string.IsNullOrWhiteSpace(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
