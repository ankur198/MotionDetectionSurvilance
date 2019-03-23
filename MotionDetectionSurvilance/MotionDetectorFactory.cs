using MotionDetectionSurvilance.Web;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;


namespace MotionDetectionSurvilance
{
    internal class MotionDetectorFactory
    {
        private readonly CameraSettings CameraSettings;
        private SoftwareBitmap oldImg;
        private readonly NetworkManager NetworkManager;
        private readonly MotionDetector MotionDetector;
        private readonly MotionDataCollection MotionDataCollection;

        public MotionDetectorFactory(CameraSettings cameraSettings, SoftwareBitmap oldImg,
            NetworkManager networkManager, MotionDetector motionDetector,
            MotionDataCollection motionDataCollection)
        {
            CameraSettings = cameraSettings;
            this.oldImg = oldImg;
            NetworkManager = networkManager;
            MotionDetector = motionDetector;
            MotionDataCollection = motionDataCollection;
        }

        internal async void CaptureImage(int threshold, int smooth)
        {
            //capture new image
            SoftwareBitmap newImage = await CameraSettings.cameraPreview.CaptureImage();

            if (newImage != null)
            {
                if (oldImg == null)
                {
                    oldImg = newImage;
                }

                var result = await Task.Factory.StartNew(() => MotionDetector.ComputeDifference(oldImg, newImage, threshold, smooth));

                //Status.Text = result.Difference.ToString();
                await MainPage.runOnUIThread(() =>
                MotionDataCollection.AddMotion(result.Difference));
                NetworkManager.Image = result.Image;

                oldImg = newImage; //update old image

                //var source = new SoftwareBitmapSource();
                //await source.SetBitmapAsync(result.Image);
                //ImgPreview.Source = source;

                ImageCaptured?.Invoke(this, result);
            }
        }
        public event EventHandler<MotionResult> ImageCaptured;
    }
}
