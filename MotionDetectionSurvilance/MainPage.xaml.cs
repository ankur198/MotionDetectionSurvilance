using MotionDetectionSurvilance.Web;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
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
            MotionDetectorFactory.ImageCaptured += MotionDetectorFactory_ImageCaptured;

            Task.Factory.StartNew(() => NetworkManager.Start());
            NetworkManager.UpdateSettings += NetworkManager_UpdateSettings;
        }

        private void MotionDetectorFactory_ImageCaptured(object sender, MotionResult e)
        {
            UpdatePrevToResult(e.Image);

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
                if (e.SettingName == SettingName.Noise)
                {
                    Noise.Value = e.Value;
                }
                else if (e.SettingName == SettingName.Multiplier)
                {
                    Multiplier.Value = e.Value;
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
    }
}
