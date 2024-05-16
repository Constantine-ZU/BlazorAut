
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
namespace BlazorAut.Services
{

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPass)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
        }


        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                };

                var mailMessage = new MailMessage(_smtpUser, to, subject, body);
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Sent {to} {subject} {body} {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email:{to} {subject} {body}  {ex.Message}");
                
            }
        }

    }

}
