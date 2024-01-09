using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace PracAssignment.Helper
{
    public class EmailSender : IEmailSender
	{
		private readonly IConfiguration _configuration;
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = _configuration["AccountEmail"];
            var password = _configuration["AccountPassword"];

			SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(mail, password)
            }; 

            return client.SendMailAsync(
                new MailMessage
                (from: mail,
                to: email,
                subject,
                htmlMessage));
        }
    }
}
