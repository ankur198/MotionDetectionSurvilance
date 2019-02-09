using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace MotionDetectionSurvilance
{
    internal class CameraSettings
    {
        private readonly CaptureElement previewControl;
        private readonly TextBlock status;
        private readonly CoreDispatcher dispatcher;

        internal MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();


        internal CameraInformation cameraInformation;
        internal CameraPreview cameraPreview;

        public ObservableCollection<CameraInformation> Cameras { get; private set; }

        internal async void ShowCameraListAsync()
        {
            Cameras = new ObservableCollection<CameraInformation>();
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            foreach (var device in devices)
            {
                Cameras.Add(new CameraInformation() { deviceInformation = device });
            }
        }

        public CameraSettings(CaptureElement previewControl, TextBlock status, CoreDispatcher dispatcher)
        {
            this.previewControl = previewControl;
            this.status = status;
            this.dispatcher = dispatcher;

            cameraPreview = new CameraPreview(previewControl, status, dispatcher);
        }

        internal void StartPreview()
        {
            cameraPreview.StartPreviewAsync(settings);
        }
    }
}
