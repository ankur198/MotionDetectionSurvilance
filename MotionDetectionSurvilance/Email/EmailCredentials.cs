using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MotionDetectionSurvilance.Web
{
    class EmailCredentials
    {
        const string KeyEmail = "EmailUser";
        const string KeyPassword = "EmailPassword";

        string _FromEmail = "";
        string _Password = "";
        public string FromEmail { get { return _FromEmail; } set { _FromEmail = value; UpdateValues(this); } }
        public string Password { get { return _Password; } set { _Password = value; UpdateValues(this); } }


        private static void UpdateValues(EmailCredentials credentials)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[KeyEmail] = credentials.FromEmail;
            localSettings.Values[KeyPassword] = credentials.Password;
        }

        public static EmailCredentials GetValues()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var c = new EmailCredentials();
            if (localSettings.Values[KeyEmail] == null)
            {
                return null;
            }
            c._FromEmail = localSettings.Values[KeyEmail].ToString();
            c._Password = localSettings.Values[KeyPassword].ToString();

            return c;
        }
    }
}
