using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace GamingThroughVoiceRecognitionSystem.Services
{
    public class FaceRecognitionService
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap currentFrame;

        public event EventHandler<Bitmap> FrameCaptured;
        public event EventHandler CameraStarted;
        public event EventHandler CameraStopped;

        public FaceRecognitionService()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public bool IsCameraAvailable()
        {
            return videoDevices.Count > 0;
        }

        public void StartCamera()
        {
            if (!IsCameraAvailable())
            {
                throw new Exception("No camera device found!");
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            CameraStarted?.Invoke(this, EventArgs.Empty);
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            currentFrame = (Bitmap)eventArgs.Frame.Clone();
            FrameCaptured?.Invoke(this, currentFrame);
        }

        public byte[] CaptureFace()
        {
            if (currentFrame == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                currentFrame.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource.NewFrame -= VideoSource_NewFrame;
                CameraStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        public BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                bitmapimage.Freeze();
                return bitmapimage;
            }
        }
    }
}
