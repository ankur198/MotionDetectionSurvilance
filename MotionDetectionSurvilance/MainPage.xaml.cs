using MotionDetectionSurvilance.Web;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace MotionDetectionSurvilance
{
    public sealed partial class MainPage : Page
    {
        private CameraSettings CameraSettings;
        private SoftwareBitmap oldImg;
        private MotionDataCollection MotionDataCollection;
        private int threshold;
        private int smooth;
        private MotionDetector MotionDetector;

        private MotionDetectorFactory MotionDetectorFactory;

        private NetworkManager NetworkManager;

        public MainPage()
        {
            this.InitializeComponent();
            myDispatcher = Dispatcher;

            threshold = 25;
            smooth = 10;

            CameraSettings = new CameraSettings(PreviewControl, Status, Dispatcher);
            CameraSettings.ShowCameraListAsync();
            CamerasList.SelectedIndex = 0;
            CameraSettings.cameraPreview.PreviewStatusChanged += CameraPreview_PreviewStatusChanged;

            MotionDataCollection = new MotionDataCollection();
            ChartDiagnostic.DataContext = MotionDataCollection.MotionValue;

            MotionDetector = new MotionDetector();

            NetworkManager = new NetworkManager();

            MotionDetectorFactory = new MotionDetectorFactory(CameraSettings, oldImg, NetworkManager,
                MotionDetector, MotionDataCollection);

            MotionDetectorFactory.ImageCaptured += MotionDetectorFactory_ImageCaptured;

            

            Task.Factory.StartNew(() => NetworkManager.Start());
            //NetworkManager.Start();

            NetworkManager.UpdateSettings += NetworkManager_UpdateSettings;

        }

        private void MotionDetectorFactory_ImageCaptured(object sender, MotionResult e)
        {
            CaptureImage();
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
            CameraSettings.settings.VideoDeviceId = selectedCamera.deviceInformation.Id;

            CameraSettings.StartPreview();
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
                   await Task.Factory.StartNew(()=> MotionDetectorFactory.CaptureImage(threshold, smooth));
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
    }
}
