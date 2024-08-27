using CodeShareBackend.Models;

namespace CodeShareBackend.IServices
{
    public interface IMailService
    {
        bool SendMail(MailData Mail_Data);
    }
}
