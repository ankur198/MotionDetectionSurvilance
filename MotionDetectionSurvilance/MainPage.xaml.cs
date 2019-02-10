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

        public MainPage()
        {
            this.InitializeComponent();

            threshold = 25;
            smooth = 10;

            CameraSettings = new CameraSettings(PreviewControl, Status, Dispatcher);
            CameraSettings.ShowCameraListAsync();
            CamerasList.SelectedIndex = 0;
            CameraSettings.cameraPreview.PreviewStatusChanged += CameraPreview_PreviewStatusChanged;

            MotionDataCollection = new MotionDataCollection();
            ChartDiagnostic.DataContext = MotionDataCollection.MotionValue;
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
            CameraSettings.settings.VideoDeviceId = selectedCamera.deviceInformation.Id;

            CameraSettings.StartPreview();
        }

        private async void BtnCapture_Click(object sender, RoutedEventArgs e)
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
                    //Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
        }

        private SoftwareBitmap image;

        private async Task CaptureImage()
        {
            image = await CameraSettings.cameraPreview.CaptureImage();

            if (image != null)
            {
                if (oldImg == null)
                {
                    oldImg = image;
                }

                var result = await Task.Factory.StartNew(() => new MotionDetector().ComputeDifference(oldImg, image, threshold, smooth));
                result.Difference = result.Difference / smooth;
                Status.Text = result.Difference.ToString();

                MotionDataCollection.AddMotion(Math.Abs(result.Difference));
                //ChartDiagnostic.DataContext = MotionDataCollection.MotionValue;
                //ChartDiagnostic.UpdateLayout();

                oldImg = result.Image;

                image = SoftwareBitmap.Convert(result.Image, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                var source = new SoftwareBitmapSource();

                await source.SetBitmapAsync(image);

                ImgPreview.Source = source;
            }
        }

        private async Task runOnUIThread(DispatchedHandler d)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, d);
        }
    }
}
