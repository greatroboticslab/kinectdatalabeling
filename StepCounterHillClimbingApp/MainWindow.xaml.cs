using System.Configuration;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using VirginiaTech.Fuse.Render;
using VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.StepCount
{
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private VideoRenderer videoRenderer;
        private int elevationAngleAdjust;
        private static readonly string rightLearningFileName = "rightLearning.json";
        private static readonly string leftLearningFileName = "leftLearning.json";
        private static readonly string trainingDirectory = "StepCountData";

        public MainWindow()
        {
            InitializeComponent();

            checkBoxSeatedMode.Checked += (object sender, RoutedEventArgs e) =>
                {
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                };
            checkBoxSeatedMode.Unchecked += (object sender, RoutedEventArgs e) =>
                {
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                };
            nearModeCheckbox.Checked += (object sender, RoutedEventArgs e) =>
                {
                    sensor.SkeletonStream.EnableTrackingInNearRange = true;
                };

            nearModeCheckbox.Unchecked += (object sender, RoutedEventArgs e) =>
                {
                    sensor.SkeletonStream.EnableTrackingInNearRange = false;
                };
            angleDownLabel.Click += (object sender, RoutedEventArgs e) =>
                {
                    elevationAngleAdjust = Math.Max(elevationAngleAdjust - 1, sensor.MinElevationAngle);
                    angleCounterLabel.Content = elevationAngleAdjust;
                };
            angleUpLabel.Click += (object sender, RoutedEventArgs e) =>
                {
                    elevationAngleAdjust = Math.Min(elevationAngleAdjust + 1, sensor.MaxElevationAngle);
                    angleCounterLabel.Content = elevationAngleAdjust;
                };
            executeAngleButton.Click += (object sender, RoutedEventArgs e) =>
                {
                    sensor.ElevationAngle = elevationAngleAdjust;
                };

            Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
                {
                    videoRenderer.pauseRecording();
                    sensor.Stop();
                };
        }

        private KinectSensor getSensor()
        {
            while (true)
            {
                foreach (KinectSensor s in KinectSensor.KinectSensors)
                {
                    return s;
                }
            }
        }

        private static double[] loadTrainingData(string filePath)
        {
            using (var stream = new StreamReader(Path.Combine(Util.FileControl.ExperimentDirectory, trainingDirectory, filePath)))
            {
                return JsonConvert.DeserializeObject<List<double>>(stream.ReadToEnd()).ToArray<double>();
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            sensor = getSensor();

            double[] rightLearningVector = loadTrainingData(rightLearningFileName);
            double[] leftLearningVector = loadTrainingData(leftLearningFileName);
            StepCounterMachineLearning stepCounter = new StepCounterMachineLearning(rightLearningVector, leftLearningVector);

            sensor.AllFramesReady += drawFrame;
            sensor.SkeletonFrameReady += (kinectSender, readyFrame) => updateSteps(stepCountLabel, stepCounter, kinectSender, readyFrame);
            videoRenderer = new VideoRenderer(sensor, 640, 480);
            sensor.ColorStream.Enable();
            sensor.SkeletonStream.Enable();
            sensor.Start();
            angleCounterLabel.Content = sensor.ElevationAngle;
            elevationAngleAdjust = sensor.ElevationAngle;
        }

        private void drawFrame(object sender, AllFramesReadyEventArgs readyFrame)
        {
            Image.Source = new RenderFrame(readyFrame, sensor.CoordinateMapper, 640, 480).render();
        }

        private static void updateSteps(System.Windows.Controls.Label label, StepCounterMachineLearning stepCounter,
            object sender, SkeletonFrameReadyEventArgs readyFrame)
        {
            if (readyFrame != null)
                stepCounter.processFrameData(sender, readyFrame);

            label.Content = stepCounter.count().ToString();
        }
    }
}