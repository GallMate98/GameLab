using GameLab.Models;

namespace GameLab.Services.Email
{
    public interface IEmailService
    {
        void SendMail(EmailDto email);
    }
}
