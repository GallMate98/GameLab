using EASendMail;
using GameLab.Models;

namespace GameLab.Services.Email
{
    public class EmailService:IEmailService
    { 
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration config)
        {
            _configuration = config;
        }
        public void SendMail(EmailDto email) 
        {
            IConfigurationSection emailConfig = _configuration.GetSection("Email");
            string fromEmail = emailConfig["From"];
            string password = emailConfig["Password"];

            try
            {
                SmtpMail oMail = new SmtpMail("TryIt");
                oMail.From = fromEmail; 
                oMail.To = email.To;
                oMail.Subject = email.Subject;
                oMail.HtmlBody = $"<p>{email.Body}</p>";
                SmtpServer oServer = new SmtpServer("smtp.mail.yahoo.com");
                oServer.User = fromEmail;
                oServer.Password = password;
                oServer.Port = 465;
                oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;
                EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
                oSmtp.SendMail(oServer, oMail);
            }
            catch (Exception exception)
            {
                Console.WriteLine("failed to send email with the following error:\n" + exception.Message);
            }
        }
    }
}
