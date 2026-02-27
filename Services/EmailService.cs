using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace HazelnutVeb.Services
{
    public class EmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, "Hazelnut Arena");
            var toEmail = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"SendGrid status: {response.StatusCode}");
        }
    }
}
