using System;
using System.Net;
using System.Net.Mail;

namespace ManualImplementation
{
    class Program
    {
        static void Main(string[] args)
        {
            send();
            Console.WriteLine("Hello World!");
        }
        static void send()
        {
            var html = "<h1>hii</h1>";
            var msg = new MailMessage("ankur.nigam198@gmail.com", "ankur.nigam198@live.com", "Something moved", html);
            msg.IsBodyHtml = true;
            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            //smtpClient.UseDefaultCredentials = true;
            smtpClient.Credentials = new NetworkCredential("ankur.nigam198@gmail.com", "asdLkj654");
            smtpClient.EnableSsl = true;
            smtpClient.SendAsync(msg, null);
            //smtpClient.Send(msg);
            Console.WriteLine("Email Sent Successfully");
        }
    }
}
