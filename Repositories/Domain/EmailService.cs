using Microsoft.Extensions.Options;
using Spider_QAMS.Repositories.Skeleton;
using System.Net.Mail;
using System.Net;
using Spider_QAMS.Models;

namespace Spider_QAMS.Repositories.Domain
{
    public class EmailService: IEmailService
    {
        private readonly IOptions<SmtpSetting> _smtpSetting;
        public EmailService(IOptions<SmtpSetting> smtpSetting)
        {
            _smtpSetting = smtpSetting;
        }
        public async Task SendAsync(string from, string to, string subject, string body)
        {
            var message = new MailMessage(from, to, subject, body);
            using (var emailClient = new SmtpClient(_smtpSetting.Value.Host, _smtpSetting.Value.Port))
            {
                emailClient.Credentials = new NetworkCredential(_smtpSetting.Value.User, _smtpSetting.Value.Password);
                await emailClient.SendMailAsync(message);
            }
        }
    }
}
