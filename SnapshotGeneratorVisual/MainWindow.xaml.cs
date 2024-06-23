using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using VirginiaTech.Fuse.Render;
using VirginiaTech.Fuse.Util;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using System.IO;
using Util = VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Record
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private VideoRenderer videoRenderer;
        private KinectAngleAdjuster angleAdjuster;
        private PositionRecorder positionRecorder;
        private Counter frameCounter;
        private BufferBlock<ImageToSave> saveImageBuffer;
        private Random random;
        private string sessionDirectory;

        public MainWindow()
        {
            InitializeComponent();

            sensor = ExperimentUtil.connectToKinect();
            
            // Turn on sensor and set up sensor callbacks.
            videoRenderer = new VideoRenderer(sensor, 640, 480);
            sensor.AllFramesReady += DrawFrame;
            sensor.Start();
            sensor.AllFramesReady += (_sender, readyFrame) => processFrameData(sensor.CoordinateMapper, _sender, readyFrame);
            sensor.SkeletonStream.Enable();
            sensor.ColorStream.Enable();

            random = new Random();

            // Set up async image saving.
            saveImageBuffer = new BufferBlock<ImageToSave>();
            SaveImages(saveImageBuffer);

            angleAdjuster = new KinectAngleAdjuster(sensor);
            frameCounter = new Counter(0);
            positionRecorder = new PositionRecorder();

            // Set up buttons.
            SetUpRecordingGUI();
            SetUpAngleAdjusterGUI(angleAdjuster, angleUpButton, angleDownButton, executeAngleButton, angleCounterLabel);
            Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                sensor.Stop();
                saveImageBuffer.Complete();
            };
        }

        /// <summary>
        /// Set the callbacks for the recording GUI.
        /// </summary>
        private void SetUpRecordingGUI()
        {
            recordToggleButton.Click += (object sender, RoutedEventArgs e) =>
            {
                sessionDirectory = FileControl.UniqueExperimentFolder(folderPrefixTextBox.Text);
                if (positionRecorder.Recording)
                {
                    recordToggleButton.Content = "Start";
                    frameCounterButton.IsEnabled = true;
                    frameCounterText.IsReadOnly = false;
                    positionRecorder.Recording = false;
                }
                else
                {
                    recordToggleButton.Content = "Stop";
                    positionRecorder.SaveAndClear(FileControl.UniquePositionsFileName(sessionDirectory));
                    frameCounterButton.IsEnabled = false;
                    frameCounterText.IsReadOnly = true;
                    positionRecorder.Recording = true;
                }
            };
            frameCounterButton.Click += (object sender, RoutedEventArgs e) =>
            {
                if (positionRecorder.Recording)
                {
                    frameCounterButton.IsEnabled = true;
                    recordToggleButton.IsEnabled = true;
                    frameCounterText.IsReadOnly = false;
                    positionRecorder.Recording = false;
                }
                else
                {
                    try
                    {
                        frameCounter = new Counter(Int32.Parse(frameCounterText.Text));
                        frameCounterButton.IsEnabled = false;
                        recordToggleButton.IsEnabled = false;
                        frameCounterText.IsReadOnly = true;
                        positionRecorder.Recording = true;
                        sessionDirectory = FileControl.UniqueExperimentFolder(folderPrefixTextBox.Text);
                    }
                    catch (System.FormatException)
                    {
                        frameCounterText.Text = frameCounter.CountLeft.ToString();
                    }
                }
            };
        }

        /// <summary>
        /// Set the callbacks for the angle adjuster GUI.
        /// </summary>
        /// <param name="angleAdjuster">A driver to the sensor.</param>
        /// <param name="upButton">The button for changing the next position to a higher angle.</param>
        /// <param name="downButton">The button for changing the next position to a lower angle.</param>
        /// <param name="executeButton">The button for changing the position to the current angle.</param>
        /// <param name="counterLabel">The label showing the angle to move to when executed.</param>
        private static void SetUpAngleAdjusterGUI(KinectAngleAdjuster angleAdjuster, Button upButton, Button downButton, Button executeButton,
            System.Windows.Controls.Label counterLabel)
        {
            counterLabel.Content = angleAdjuster.ElevationAngle;
            downButton.Click += (object sender, RoutedEventArgs e) =>
            {
                angleAdjuster.AngleDown();
                counterLabel.Content = angleAdjuster.ElevationAdjustAngle;
            };
            upButton.Click += (object sender, RoutedEventArgs e) =>
            {
                angleAdjuster.AngleUp();
                counterLabel.Content = angleAdjuster.ElevationAdjustAngle;
            };
            executeButton.Click += (object sender, RoutedEventArgs e) =>
            {
                angleAdjuster.Execute();
            };
        }

        private void DrawFrame(object sender, AllFramesReadyEventArgs readyFrame)
        {
            Image.Source = new RenderFrame(readyFrame, sensor.CoordinateMapper, 640, 480).render();
        }

        /// <summary>
        /// Returns true or false randomly.
        /// </summary>
        /// <param name="n">The liklihood that true will be returned. The larger 'n' is, the less likely true will be returned.</param>
        /// <returns>True or false.</returns>
        private bool Sometimes(int n)
        {
            return random.Next(n) == 0;
        }



        private static async void SaveImages(ISourceBlock<ImageToSave> source)
        {
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
            while (await source.OutputAvailableAsync())
            {
                ImageToSave imageToSave = source.Receive();
                using (Stream imageStream = File.Create(imageToSave.FilePath))
                {
                    jpegEncoder.Frames.Add(imageToSave.Bitmap);
                    jpegEncoder.Save(imageStream);
                }
                jpegEncoder = new JpegBitmapEncoder();
            }
        }

        private Skeleton[] GetSkeletonData(AllFramesReadyEventArgs readyFrame)
        {
            Skeleton[] skeletons;
            using (SkeletonFrame skeletonFrame = readyFrame.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return null;
                }
                skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength]; //TODO This breaks the GC.
                skeletonFrame.CopySkeletonDataTo(skeletons);
            }
            return skeletons;
        }


        /// <summary>
        /// A method to be used as a Kinect event handler. It will copy and save a
        /// frame of video and skeleton data every so often. The frame of video is
        /// a bitmap image. The skeleton data is a vector of values calculated using
        /// joint objects. See the source code for the exact calculations.
        /// </summary>
        /// <param name="coordMapper">Used for image rendering.</param>
        /// <param name="stream">The comma seperated values file to record data to.</param>
        /// <param name="sender">Kinect related. Is not used.</param>
        /// <param name="readyFrame">A frame worth of Kinect data.</param>
        private void processFrameData(CoordinateMapper coordMapper,
            object sender, AllFramesReadyEventArgs readyFrame)
        {
            Skeleton[] skeletons = GetSkeletonData(readyFrame);
            if (skeletons == null)
            {
                return;
            }
            foreach (Skeleton skeleton in skeletons.Where(skel => skel.TrackingState == SkeletonTrackingState.Tracked))
            {
                // Only save some frames. TODO Get rid of this or make it take snapshots in constant interval
                if (!Sometimes(10))
                {
                    continue;
                }

                if (frameCounter.Complete() && positionRecorder.Recording)
                {
                    positionRecorder.SaveAndClear(FileControl.UniquePositionsFileName(sessionDirectory));
                    positionRecorder.Recording = false;
                    frameCounterButton.Content = "Start";
                    frameCounterText.IsReadOnly = false;
                    frameCounterButton.IsEnabled = true;
                    recordToggleButton.IsEnabled = true;
                }
                else if (positionRecorder.Recording)
                {
                    string imageFileName = FileControl.UniqueImageFileName(sessionDirectory);

                    // Save image data.
                    saveImageBuffer.Post(new ImageToSave(
                        imageFileName,
                        BitmapFrame.Create(new RenderFrame(readyFrame, coordMapper, 640, 480).render())));

                    // Save position data
                    Dictionary<JointType, Vector3> sourceJointPositionDict = new Dictionary<JointType, Vector3>();
                    foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
                    {
                        sourceJointPositionDict[jointType] = ExperimentUtil.skeletonPointToVector(skeleton.Joints[jointType].Position);
                    }
                    positionRecorder.AddFrame(new VirginiaTech.Fuse.Util.JointPositions(System.IO.Path.GetFileName(imageFileName), sourceJointPositionDict));

                    frameCounter.Count();
                    frameCounterText.Text = frameCounter.CountLeft.ToString();
                }
            }
        }
    }

    /// <summary>
    /// Represents a message containing an image to save.
    /// </summary>
    public struct ImageToSave
    {
        public readonly string FilePath;
        public readonly BitmapFrame Bitmap;

        public ImageToSave(string filePath, BitmapFrame bitmap)
        {
            FilePath = filePath;
            Bitmap = bitmap;
        }
    }
}
