using MotionDetectionSurvilance.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MotionDetectionSurvilance
{
    public sealed partial class MainPage : Page
    {
        private CoreCamera Camera;
        internal static SoftwareBitmap oldImg;
        private MotionDataCollection MotionDataCollection;
        private int threshold;
        private int smooth;

        private MotionDetectorFactory MotionDetectorFactory;

        private NetworkManager NetworkManager;

        public MainPage()
        {
            this.InitializeComponent();
            myDispatcher = Dispatcher;
            myStatus = Status;

            threshold = 25;
            smooth = 10;

            Camera = new CoreCamera(PreviewControl);

            CamerasList.SelectedIndex = 0;
            Camera.cameraPreview.PreviewStatusChanged += CameraPreview_PreviewStatusChanged;

            MotionDataCollection = new MotionDataCollection(20);
            MotionChart.DataContext = MotionDataCollection.MotionValue;

            NetworkManager = new NetworkManager();

            MotionDetectorFactory = new MotionDetectorFactory(Camera, MotionDataCollection);
            MotionDetectorFactory.ImageCaptured += UpdateUI;
            MotionDetectorFactory.ImageCaptured += SendNotification;
            MotionDetectorFactory.ImageCaptured += SaveImage;

            Task.Factory.StartNew(() => NetworkManager.Start());
            NetworkManager.UpdateSettings += NetworkManager_UpdateSettings;
        }

        private async void SaveImage(object sender, MotionResult e)
        {
            int notification = 999999;
            bool? isNotification = false;
            await runOnUIThread(() => { notification = (int)NotificationAt.Value; isNotification = NotificationEnable.IsChecked; });
            if (e.Difference > notification && isNotification == true)
            {
                MotionDetectorFactory.SaveImage();
                Debug.WriteLine("Image Captured");
            }

            await runOnUIThread(() =>
            {
                if (e.Difference > notification)
                {
                    NotificationControl.Background = new SolidColorBrush(Color.FromArgb(255, 244, 217, 66));
                }
                else
                {
                    NotificationControl.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                }
            });
        }


        private void SendNotification(object sender, MotionResult e)
        {
            new Task(async () =>
            {
                int notification = 999999;
                bool? isNotification = false;
                await runOnUIThread(() => { notification = (int)NotificationAt.Value; isNotification = NotificationEnable.IsChecked; });
                if (e.Difference > notification && isNotification == true)
                {
                    //big movement occured
                    SubscribeNotificationData.sendNotificationToAll();
                    MotionDetectorFactory.ImageCaptured -= SendNotification;
                    Task.Delay(5000).Wait();
                    MotionDetectorFactory.ImageCaptured += SendNotification;
                }
            }).Start();
        }

        private void UpdateUI(object sender, MotionResult e)
        {
            UpdatePrevToResult(e.Image);
            ShowMessage(e.Difference.ToString());
            CaptureImage();


        }

        private async void UpdatePrevToResult(SoftwareBitmap image)
        {
            await runOnUIThread(async () =>
            {
                var softwareBitmap = image;
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                            softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                var source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(softwareBitmap);

                // Set the source of the Image control
                ImgPreview.Source = source;
            });
        }

        private async void NetworkManager_UpdateSettings(object sender, Settings e)
        {
            await runOnUIThread(() =>
            {
                switch (e.SettingName)
                {
                    case SettingName.Noise:
                        Noise.Value = e.Value;
                        break;
                    case SettingName.Multiplier:
                        Multiplier.Value = e.Value;
                        break;
                    case SettingName.NotificationAt:
                        NotificationAt.Value = e.Value;
                        break;
                    case SettingName.NotificationEnable:
                        NotificationEnable.IsChecked = e.Value == 0 ? false : true;
                        break;
                }
            });
        }

        private void CameraPreview_PreviewStatusChanged(object sender, bool preview)
        {
            StartPreview.Content = $"{(preview ? "Stop" : "Start")} preview";
            CamerasList.IsEnabled = !preview;
            BtnCapture.IsEnabled = preview;
        }

        private void StartPreview_ClickAsync(object sender, RoutedEventArgs e)
        {
            var selectedCamera = CamerasList.SelectedItem as CameraInformation;
            if (selectedCamera == null)
            {
                Status.Text = "No camera selected/found";
                return;
            }
            Camera.settings.VideoDeviceId = selectedCamera.deviceInformation.Id;

            Camera.StartPreview();
        }

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            //TODO: make it to click image automatically

            isMonitoring = !isMonitoring;
            //await CaptureImage();


            if (isMonitoring)
            {
                CaptureImage();
            }
        }

        private bool isMonitoring = false;

        private async void CaptureImage()
        {
            try
            {
                if (isMonitoring)
                {
                    await Task.Factory.StartNew(() => MotionDetectorFactory.CaptureImage(threshold, smooth));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
        }



        private static CoreDispatcher myDispatcher;

        public static async Task runOnUIThread(DispatchedHandler d)
        {
            await myDispatcher.RunAsync(CoreDispatcherPriority.Normal, d);
        }

        private static TextBlock myStatus;

        public static async void ShowMessage(String message)
        {
            await runOnUIThread(() => myStatus.Text = message);
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            await ApplicationData.Current.ClearAsync();
        }

        private void CamerasList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            
        }
    }
}
