using ConferenceWebApp.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Configuration;


namespace ConferenceWebApp.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string email, string subject, string message)
        {
            var host = _configuration["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host");
            var portStr = _configuration["Smtp:Port"] ?? "587";
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"] ?? username ?? throw new ArgumentNullException("Smtp:From");

            if (!int.TryParse(portStr, out var port)) port = 587;

            var secure = ParseSecureSocket(_configuration["Smtp:SecureSocket"]); // default: StartTls
            var allowInvalid = bool.TryParse(_configuration["Smtp:AllowInvalidCerts"], out var a) && a;

            // Собираем письмо
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(from));
            mimeMessage.To.Add(MailboxAddress.Parse(email));
            mimeMessage.Subject = subject;

            // Тело (HTML + текст на всякий)
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message,
                TextBody = StripHtml(message)
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            if (allowInvalid)
            {
                // НЕ используйте в продакшене
                client.ServerCertificateValidationCallback = (_, _, _, _) => true;
            }

            // Подключаемся
            await client.ConnectAsync(host, port, secure);

            // Если сервер требует аутентификацию
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                await client.AuthenticateAsync(username, password);

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }

        private static SecureSocketOptions ParseSecureSocket(string? value) =>
            value?.ToLowerInvariant() switch
            {
                "sslonconnect" => SecureSocketOptions.SslOnConnect,
                "starttls" => SecureSocketOptions.StartTls,
                "starttlswhenavailable" => SecureSocketOptions.StartTlsWhenAvailable,
                "none" => SecureSocketOptions.None,
                "auto" or null or "" => SecureSocketOptions.Auto,
                _ => SecureSocketOptions.StartTls
            };

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            // простой fallback: удаляем теги, оставляем текст
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}
