using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace MotionDetectionSurvilance
{
    internal class MotionDetector
    {
        internal long ComputeDifference(SoftwareBitmap img1, SoftwareBitmap img2)
        {
            var img1data = GetImageData(img1);
            var img2data = GetImageData(img2);


            //TODO: group pixels,get their avg difference and run for 3 filters in seperate thread
            long difference = 0;
            int Compensation = 10;
            for (int i = 0; i < img1data.blue.Length; i++)
            {
                var pixeldifference = img1data.blue[i] - img2data.blue[i];
                difference += Math.Abs(pixeldifference) - Compensation <= 0 ? 0 : pixeldifference;
            }
            return difference;
        }

        private ImageData GetImageData(SoftwareBitmap img)
        {
            byte[] buffer = new byte[4 * img.PixelHeight * img.PixelWidth];

            img.CopyToBuffer(buffer.AsBuffer());

            var ImageData = new ImageData();
            ImageData.red = new byte[buffer.Length / 4];
            ImageData.green = new byte[buffer.Length / 4];
            ImageData.blue = new byte[buffer.Length / 4];
            ImageData.alpha = new byte[buffer.Length / 4];

            int index = 0;
            for (int i = 0; i < buffer.Length; i += 4)
            {
                ImageData.red[index] = buffer[i];
                ImageData.green[index] = buffer[i + 1];
                ImageData.blue[index] = buffer[i + 2];
                ImageData.alpha[index] = buffer[i + 3];

            }

            return ImageData;
        }
    }

    internal class ImageData
    {
        public byte[] red;
        public byte[] green;
        public byte[] blue;
        public byte[] alpha;
    }
}
