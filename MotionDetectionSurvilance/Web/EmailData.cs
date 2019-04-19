using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDetectionSurvilance.Web
{
    class EmailData
    {
        static string image = "";
        public static List<EmailData> EmailList { get; set; }
        public static async void SendEmailToAll()
        {
            if (EmailList == null)
            {
                return;
            }
            image = await NetworkManager.SendImage();
            foreach (var email in EmailList)
            {
                email.SendEmail();
            }
        }

        public EmailData(string EmailTo)
        {
            this.EmailTo = EmailTo;
        }
        private string EmailTo;

        private async void SendEmail()
        {

            var html = $"<html><body><h1>Found Some movement</h1><br><img src='data:image/png;base64, {image}'/></body></html>";

            //var apiKey = "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";
            //var client = new SendGridClient(apiKey);

            //var from = new EmailAddress("ankur.nigam198@gmail.com", "Ankur Nigam");
            //var subject = "Motion found!!";
            //var to = new EmailAddress(this.EmailTo);
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, "", html);

            //var res = await client.SendEmailAsync(msg);

        }
    }
}
