using CodeShareBackend.IServices;
using CodeShareBackend.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CodeShareBackend.Services
{
    public class MailService : IMailService
    {
        MailSettings Mail_Settings;
        public MailService(IOptions<MailSettings> options)
        {
            Mail_Settings = options.Value;
        }
        public bool SendMail(MailData Mail_Data)
        {
            try
            {
                if (Mail_Settings.UserName == string.Empty)
                {
                    Console.WriteLine("Mail Settings not found in appsettings.json");
                    return false;
                }

                MimeMessage email_Message = new MimeMessage();
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
                email_Message.From.Add(email_From);
                
                MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
                email_Message.To.Add(email_To);
                email_Message.Subject = Mail_Data.EmailSubject;
                
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = Mail_Data.EmailBody;
                email_Message.Body = emailBodyBuilder.ToMessageBody();

                SmtpClient MailClient = new SmtpClient();
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
                MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);
                MailClient.Send(email_Message);
                MailClient.Disconnect(true);
                MailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
