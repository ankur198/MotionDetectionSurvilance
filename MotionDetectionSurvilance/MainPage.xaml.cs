using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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


        public MainPage()
        {
            this.InitializeComponent();

            CameraSettings = new CameraSettings(PreviewControl, Status, Dispatcher);
            CameraSettings.ShowCameraListAsync();
            CamerasList.SelectedIndex = 0;
            CameraSettings.cameraPreview.PreviewStatusChanged += CameraPreview_PreviewStatusChanged;
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
            var image = await CameraSettings.cameraPreview.CaptureImage();

            if (oldImg == null)
            {
                oldImg = image;
            }
            else
            {
                Status.Text = new MotionDetector().ComputeDifference(oldImg, image).ToString();
                oldImg = image;
            }
        }
    }
}
