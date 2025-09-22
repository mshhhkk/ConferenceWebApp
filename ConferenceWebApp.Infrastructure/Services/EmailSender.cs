using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

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
            //var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            //{
            //    Port = int.Parse(_configuration["Smtp:Port"]),
            //    Credentials = new NetworkCredential(
            //        _configuration["Smtp:Username"],
            //        _configuration["Smtp:Password"]),
            //    EnableSsl = true,
            //};

            //var fromAddress = _configuration["Smtp:From"];
            //if (string.IsNullOrEmpty(fromAddress))
            //{
            //    throw new ArgumentNullException(nameof(fromAddress), "Поле 'From' не может быть пустым.");
            //}

            //var mailMessage = new MailMessage
            //{
            //    From = new MailAddress(fromAddress),
            //    Subject = subject,
            //    Body = message,
            //    IsBodyHtml = true,
            //};

            //mailMessage.To.Add(email);

            //await smtpClient.SendMailAsync(mailMessage);
        }
    }

    /*  [HttpGet]
      public async Task<IActionResult> TestEmail([FromServices] IEmailSender emailSender)
      {
          try
          {
              await emailSender.SendAsync(
                  "grodno.smd@gmail.com", // Замените на ваш email для теста
                  "Тестовое письмо",
                  "Это тестовое письмо для проверки SMTP."
              );

              return Content("Письмо успешно отправлено!");
          }
          catch (Exception ex)
          {
              return Content($"Ошибка при отправке письма: {ex.Message}");
          }
      }*/
}