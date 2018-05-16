using log4net;
using System.Net;
using System.Net.Mail;

namespace AmazonReloader
{
    public static class Logger
    {
        public static void Info(string log)
        {
            LogManager.GetLogger(typeof(Program)).Info(log);
        }

        public static void Error(string log)
        {
            LogManager.GetLogger(typeof(Program)).Error(log);
            EmailLogger.SendLog(log);
        }
    }

    public static class EmailLogger
    {
        public static void SendLog(string log)
        {
            var fromAddress = new MailAddress("nofmxcz@gmail.com", "Amazon Reloader");
            var toAddress = new MailAddress("nofmxc@gmail.com", "Beau Gosse");
            const string fromPassword = "zbxgozmvndffdehl";
            const string subject = "Amazon Reloader Failure";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = log
            })
            {
                smtp.Send(message);
            }
        }
    }
}
