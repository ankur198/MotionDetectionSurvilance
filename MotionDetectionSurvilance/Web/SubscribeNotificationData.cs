using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPush;

namespace MotionDetectionSurvilance.Web
{
    class SubscribeNotificationData
    {
        public string endpoint { get; set; }
        public string p256dh { get; set; }
        public string auth { get; set; }

        public void SendNotification()
        {
            var pushEndpoint = endpoint;
            var p256dh = this.p256dh;
            var auth = this.auth;

            var subject = @"mailto:ankur.nigam198@gmail.com";
            const string publicKey = @"BEu09qCcFIreSF2qnR2W8pAKcFAn6wpJVFaKKx0BICpxevmLyGnrxxZFNOV0rJOyZifkgdxIxjhtNsYWREPJBNg";
            const string privateKey = @"NbLMH1eHsktglOLgiBLsD2L1eklzY1vrtlHWliAV0SU";

            var subscription = new PushSubscription(pushEndpoint, p256dh, auth);
            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
            //var gcmAPIKey = @"[your key here]";

            var webPushClient = new WebPushClient();
            try
            {
                //webPushClient.SendNotification(subscription);
                webPushClient.SendNotification(subscription, "haww koi hila", vapidDetails);
                //webPushClient.SendNotification(subscription, "payload", gcmAPIKey);
            }
            catch (WebPushException exception)
            {
                Debug.WriteLine("Http STATUS code" + exception.StatusCode);
            }
        }
    }
}
