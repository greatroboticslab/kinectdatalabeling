using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.IO;

namespace VirginiaTech.Fuse.Render
{
    /// <summary>
    /// A class for recording and saving kinect data.
    /// </summary>
    public class VideoRenderer
    {
        private int width;
        private int height;

        private KinectSensor sensor;
        private static int FPS = 10;
        private static int frameCount = 30 / FPS;
        private AForge.Video.VFW.AVIWriter tempFile;
        private string tempFileName;
        private bool isRecording = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="s">The sensor that will provide the data.</param>
        /// <param name="width">The width of the frames.</param>
        /// <param name="height">The height of the frames.</param>
        public VideoRenderer(KinectSensor sensor, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.sensor = sensor;
        }

        /// <summary>
        /// A method that saves every frame it receives to the renderFrames list.
        /// It's meant to be used as a delegate for the AllFramesReady event in
        /// the KinectSensor object.
        /// </summary>
        /// <param name="sender">The object that sent the message.</param>
        /// <param name="readyFrame">A frame of kinect data.</param>
        private void saveFrame(object sender, AllFramesReadyEventArgs readyFrame)
        {
            if (isRecording)
            {
                RenderTargetBitmap bmp = new RenderFrame(readyFrame, sensor.CoordinateMapper, width, height).render();
                using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bmp));
                    enc.Save(outStream);
                    tempFile.AddFrame(new System.Drawing.Bitmap(outStream));
                }
            }
        }

        /// <summary>
        /// Starts recording frames of kinect data. If there are any frames
        /// already recorded, they are replaced.
        /// </summary>
        public void startRecording()
        {
            tempFileName = Path.GetTempFileName() + ".avi";
            tempFile = new AForge.Video.VFW.AVIWriter();
            tempFile.Open(tempFileName, width, height);
            sensor.AllFramesReady += saveFrame;
            isRecording = true;
        }

        /// <summary>
        /// Stop recording frames of kinect data. Any recorded frames
        /// will not be cleared.
        /// </summary>
        public void pauseRecording()
        {
            isRecording = false;
        }

        /// <summary>
        /// Resumes recording frames of kinect data. If there are any frames
        /// already recorded, they are kept.
        /// </summary>
        public void resumeRecording()
        {
            isRecording = true;
        }

        /// <summary>
        /// Saves the current recorded frames into an AVI file
        /// at the specified path.
        /// </summary>
        /// <param name="fname">The path of the file to save.</param>
        public void saveVideo(String fileName)
        {
            pauseRecording();
            tempFile.Close();
            File.Copy(tempFileName, fileName);
            tempFile.Open(tempFileName, width, height);
            resumeRecording();
        }
    }
}
