using Task1.Models;

namespace Task1.Services
{
    public interface IEmailService
    {
        bool SendEmail(EmailRequest request);
    }
}
