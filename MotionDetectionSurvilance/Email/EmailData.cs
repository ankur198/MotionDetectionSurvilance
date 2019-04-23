using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MotionDetectionSurvilance.Web
{
    class EmailData
    {
        const string keyEmails = "subEmails";
        static string image = "";
        static string html = "";
        static MemoryStream streamBitmap;
        public static EmailData[] EmailList { get => getEmailList(); set => UpdateEmailList(value); }
        public static async void SendEmailToAll()
        {
            if (EmailList == null)
            {
                return;
            }
            image = await NetworkManager.SendImage();

            byte[] bitmapData = Convert.FromBase64String(FixBase64ForImage(image));
            streamBitmap = new MemoryStream(bitmapData);
            Attachment attachment = new Attachment(streamBitmap, "image.jpg");

            html = "<h1>Found Some movement</h1><br><img src='cid:MyImage'/>";

            foreach (var email in EmailList)
            {
                try
                {
                    email.SendEmail(attachment);
                }
                catch (Exception)
                {
                    // maybe wrong password, or internet issue
                }
            }

            //streamBitmap.Dispose();

            string FixBase64ForImage(string Image)
            {
                StringBuilder sbText = new StringBuilder(Image, Image.Length);
                sbText.Replace("\r\n", string.Empty); sbText.Replace(" ", string.Empty);
                return sbText.ToString();
            }
        }

        public EmailData(string EmailTo)
        {
            this.EmailTo = EmailTo;
        }
        public string EmailTo { get; private set; }

        private async void SendEmail(Attachment attachment)
        {
            var emailCredentials = EmailCredentials.GetValues();
            var msg = new MailMessage(emailCredentials.FromEmail, EmailTo, "Something moved", html);

            
            attachment.ContentId = "MyImage";
            msg.Attachments.Add(attachment);

            msg.IsBodyHtml = true;
            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(emailCredentials.FromEmail, emailCredentials.Password);
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
            Debug.WriteLine("Email Sent Successfully");
        }

        private static EmailData[] getEmailList()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var raw = localSettings.Values[keyEmails];

            if (raw == null)
            {
                return new EmailData[0];
            }
            return JsonConvert.DeserializeObject<EmailData[]>(raw as string);
        }

        private static void UpdateEmailList(EmailData[] emailDatas)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[keyEmails] = JsonConvert.SerializeObject(emailDatas);
        }
    }
}
