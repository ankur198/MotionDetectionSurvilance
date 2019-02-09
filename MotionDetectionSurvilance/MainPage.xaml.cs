using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private MotionCamera motionCamera;
        private ObservableCollection<Camera> Cameras = new ObservableCollection<Camera>();

        public MainPage()
        {
            this.InitializeComponent();
            motionCamera = new MotionCamera(PreviewControl, Status, Dispatcher);
            ShowCameraListAsync();
        }

        private void StartPreview_Click(object sender, RoutedEventArgs e)
        {
            motionCamera.StartPreviewAsync();
        }

        private async void ShowCameraListAsync()
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            foreach (var device in devices)
            {
                Cameras.Add(new Camera() { deviceInformation = device });
            }
        }
    }
}
