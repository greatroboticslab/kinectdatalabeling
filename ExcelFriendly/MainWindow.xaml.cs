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
using System.IO;
using Newtonsoft.Json;
using Microsoft.Kinect;

using VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Excel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string noFileMessage = "<None Selected>";

        private string sourceFilePath;
        private string destFilePath;
        private string sessionDirectory;

        public MainWindow()
        {
            InitializeComponent();
            sessionDirectory = FileControl.ExperimentDirectory;
            sourceFileLabel.Content = noFileMessage;
            destFileLabel.Content = noFileMessage;

            sourceFileButton.Click += (object sender, RoutedEventArgs e) =>
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.InitialDirectory = FileControl.ExperimentDirectory;
                bool? valid = dialog.ShowDialog();
                if (valid == true) // Yes, it has to be written explicitly to work.
                {
                    sourceFileLabel.Content = dialog.FileName;
                    sourceFilePath = dialog.FileName;
                }
                else
                {
                    sourceFileLabel.Content = noFileMessage;
                    sourceFilePath = null;
                }
                PermitSubmitButton();
            };
            destFileButton.Click += (object sender, RoutedEventArgs e) =>
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.InitialDirectory = FileControl.ExperimentDirectory;
                dialog.FileName = "learning";
                dialog.DefaultExt = ".csv";
                bool? valid = dialog.ShowDialog();
                if (valid == true) // Yes, it has to be written explicitly to work.
                {
                    destFileLabel.Content = dialog.FileName;
                    destFilePath = dialog.FileName;
                }
                else
                {
                    destFileLabel.Content = noFileMessage;
                    destFilePath = null;
                }
                PermitSubmitButton();
            };
            convertButton.Click += (object sender, RoutedEventArgs e) =>
            {
                List<LabelJointPositions> frames;
                using (var reader = new StreamReader(sourceFilePath))
                {
                    frames = JsonConvert.DeserializeObject<List<LabelJointPositions>>(reader.ReadToEnd());
                }
                using (var writer = new StreamWriter(destFilePath))
                {
                    writer.Write("imagefile, label");
                    foreach (var jointType in Enum.GetValues(typeof(JointType)).Cast<JointType>())
                    {
                        writer.Write(String.Format(",{0}-x,{0}-y,{0}-z", jointType));
                    }
                    writer.WriteLine();
                    foreach (var frame in frames)
                    {
                        writer.Write(String.Format("{0},{1}", frame.FileName, frame.Label));
                        foreach (var jointType in Enum.GetValues(typeof(JointType)).Cast<JointType>())
                        {
                            var vector = frame[jointType];
                            writer.Write(String.Format(",{0}, {1}, {2}", vector.X, vector.Y, vector.Z));
                        }
                        writer.WriteLine();
                    }
                }
            };
        }

        private void PermitSubmitButton()
        {
            convertButton.IsEnabled = (sourceFilePath != null) && (destFilePath != null);
        }
    }
}
