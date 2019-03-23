using MotionDetectionSurvilance.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MotionDetectionSurvilance
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CameraSettings CameraSettings;
        private SoftwareBitmap oldImg;
        private MotionDataCollection MotionDataCollection;
        private int threshold;
        private int smooth;
        private MotionDetector MotionDetector;

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
            NetworkManager.Start();

            NetworkManager.UpdateSettings += NetworkManager_UpdateSettings;

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
                startCaptureImage();
            }
        }

        private bool isMonitoring = false;

        private async void startCaptureImage()
        {
            try
            {
                while (isMonitoring)
                {
                    await CaptureImage();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
        }

        private SoftwareBitmap newImage;


        private async Task CaptureImage()
        {
            //capture new image
            newImage = await CameraSettings.cameraPreview.CaptureImage();

            if (newImage != null)
            {
                if (oldImg == null)
                {
                    oldImg = newImage;
                }

                var result = await Task.Factory.StartNew(() => MotionDetector.ComputeDifference(oldImg, newImage, threshold, smooth));

                Status.Text = result.Difference.ToString();
                MotionDataCollection.AddMotion(result.Difference);

                oldImg = newImage; //update old image

                var source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(result.Image);
                ImgPreview.Source = source;
            }
        }

        private static CoreDispatcher myDispatcher;

        public static async Task runOnUIThread(DispatchedHandler d)
        {
            await myDispatcher.RunAsync(CoreDispatcherPriority.Normal, d);
        }
    }
}
