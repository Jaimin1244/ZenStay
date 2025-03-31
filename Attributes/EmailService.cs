using System;
using System.Net.Mail;

namespace Learn_Auth.Attributes
{
    public static class EmailService
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            mail.From = new MailAddress("hotelms001@gmail.com");

            SmtpClient smtp = new SmtpClient();
            smtp.Send(mail);
        }
    }
}