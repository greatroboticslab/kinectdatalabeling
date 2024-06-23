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
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using Newtonsoft.Json;
using Util = VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Label
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerable<Util.LabelJointPositions> jointPositionsFrames;
        private int currentJointPositionsIndex;
        private Util.LabelJointPositions currentJointPositions;
        private string sessionDirectory;

        private static readonly string outputFileName = "classified.csv";
        private static readonly string rawFileName = "raw.csv";

        /// <summary>
        /// Switch to the next image in the file. if there is no
        /// next image, do nothing.
        /// </summary>
        private void NextImage()
        {
            if (currentJointPositionsIndex < jointPositionsFrames.Count() - 1)
            {
                currentJointPositionsIndex++;
            }
            currentJointPositions = jointPositionsFrames.ElementAt(currentJointPositionsIndex);
            UpdateImage();
        }

        /// <summary>
        /// Switch to the previous image in the file. If there is no
        /// previous image, do nothing.
        /// </summary>
        private void PreviousImage()
        {
            if (currentJointPositionsIndex > 0)
            {
                currentJointPositionsIndex--;
            }
            currentJointPositions = jointPositionsFrames.ElementAt(currentJointPositionsIndex);
            UpdateImage();
        }

        public MainWindow()
        {
            InitializeComponent();

            // Pick the Data Folder.
            var inDialog = new FolderBrowserDialog();
            inDialog.SelectedPath = Util.FileControl.ExperimentDirectory;
            var result = inDialog.ShowDialog(); //TODO! (What if result is false?)
            sessionDirectory = inDialog.SelectedPath;

            // Setup GUI Callback
            setUpHotKeys();
            setUpButtonBindings();
            
            // Point to the first image. TODO! (What if there are no images?)
            jointPositionsFrames = ReadRawData(System.IO.Path.Combine(sessionDirectory, rawFileName));
            currentJointPositions = jointPositionsFrames.First();
            currentJointPositionsIndex = 0;
            UpdateImage();
        }

        /// <summary>
        /// Bind all the hotkeys in the program to callbacks.
        /// </summary>
        private void setUpHotKeys()
        {
            this.KeyDown += (object sender, System.Windows.Input.KeyEventArgs e) =>
            {
                if (e.Key == System.Windows.Input.Key.J)
                {
                    currentJointPositions.Label = Util.Label.Yes;
                    swingYesButton.Focus();
                }
                else if (e.Key == System.Windows.Input.Key.L)
                {
                    currentJointPositions.Label = Util.Label.No;
                    swingNoButton.Focus();
                }
                else if (e.Key == System.Windows.Input.Key.A)
                {
                    PreviousImage();
                }
                else if (e.Key == System.Windows.Input.Key.D)
                {
                    NextImage();
                }
            };
        }

        /// <summary>
        /// Bind all the buttons and labels in the program to callbacks.
        /// </summary>
        private void setUpButtonBindings()
        {
            nextImageButton.Click += (object sender, RoutedEventArgs e) =>
            {
                NextImage();
            };
            prevImageButton.Click += (object sender, RoutedEventArgs e) =>
            {
                PreviousImage();
            };
            swingYesButton.Click += (object sender, RoutedEventArgs e) =>
            {
                currentJointPositions.Label = Util.Label.Yes;
            };
            swingNoButton.Click += (object sender, RoutedEventArgs e) =>
            {
                currentJointPositions.Label = Util.Label.No;
            };
            currentFrameTextBox.KeyDown += (object sender, System.Windows.Input.KeyEventArgs e) =>
            {
                if (e.Key == Key.Enter)
                {
                    // Passes frameNumber by reference so there's no need for ugly
                    // exception catching code.
                    int jumpNum;
                    if (Int32.TryParse(currentFrameTextBox.Text, out jumpNum) && (0 <= jumpNum && jumpNum < jointPositionsFrames.Count()))
                    {
                        currentJointPositionsIndex = jumpNum - 1;
                        currentJointPositions = jointPositionsFrames.ElementAt(currentJointPositionsIndex);
                    }
                    UpdateImage();
                }
            };
            Closing += (object sender, CancelEventArgs e) =>
            {
                WriteClassifiedData(jointPositionsFrames, System.IO.Path.Combine(sessionDirectory, outputFileName));
            };
        }

        /// <summary>
        /// Update the display to show the current joint positions data.
        /// </summary>
        private void UpdateImage()
        {
            displayImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(sessionDirectory, currentJointPositions.FileName)));
            currentFrameTextBox.Text = String.Format("<{0}/{1}>", currentJointPositionsIndex + 1, jointPositionsFrames.Count());
        }

        /// <summary>
        /// Read the raw data from a file path.
        /// </summary>
        /// <param name="rawFileLocation">The abosulte path to the raw file.</param>
        /// <returns>Return a collection of ClassifiedFeature objects containing the data.</returns>
        private static IEnumerable<Util.LabelJointPositions> ReadRawData(string rawFileLocation)
        {
            using (var reader = new StreamReader(rawFileLocation))
            {
                var jointPositionsFrames = new List<Util.LabelJointPositions>();
                var text = reader.ReadToEnd();
                var unlabelledJointPositionsFrames = JsonConvert.DeserializeObject<List<Util.JointPositions>>(text);
                foreach (var unlabelledJointPositions in unlabelledJointPositionsFrames)
                {
                    jointPositionsFrames.Add(new Util.LabelJointPositions(Util.Label.Null, unlabelledJointPositions));
                }
                return jointPositionsFrames;
            }
        }

        /// <summary>
        /// Write the classified data.
        /// </summary>
        /// <param name="featureVectors">The collection of data to write out.</param>
        private static void WriteClassifiedData(IEnumerable<Util.LabelJointPositions> jointPositionsFrames, string outputFileName)
        {
            using (var writer = new StreamWriter(outputFileName))
            {
                writer.Write(JsonConvert.SerializeObject(jointPositionsFrames, Formatting.Indented));
            }
        }
    }
}
