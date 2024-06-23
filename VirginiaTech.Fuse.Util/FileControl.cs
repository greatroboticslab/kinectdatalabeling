using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirginiaTech.Fuse.Util
{
    /// <summary>
    /// Provides a series of functions for generating folder and file names for experiment data.
    /// </summary>
    public class FileControl
    {
        static readonly string imageFilePrefix = "rawImage";
        static readonly string imageFileSuffix = ".png";
        static private Random random;
        static private int fileCount = 0;

        /// <summary>
        /// The directory where experiement dat should be stored.
        /// </summary>
        public static string ExperimentDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KinectExperiment");
            }
        }

        public static string RelevantJointsFile
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KinectExperiment",
                    "RelevantJoints.json");
            }
        }

        static FileControl()
        {
            random = new Random();
        }

        /// <summary>
        /// Returns the name of a unique folder for holding experiment data.
        /// </summary>
        /// <param name="prefix">A suffix for the folder. The name may be unique but it can be trailed with a meaningful name.</param>
        /// <returns>The name of a unique folder for holding experiment data.</returns>
        public static string UniqueExperimentFolder(string suffix)
        {
            string timeStamp = getTimeStamp();
            Directory.CreateDirectory(Path.Combine(ExperimentDirectory, timeStamp + suffix));
            return Path.Combine(ExperimentDirectory, timeStamp + suffix);
        }

        /// <summary>
        /// Returns a timestamp that can be used as part of or as a file name.
        /// </summary>
        /// <returns></returns>
        private static string getTimeStamp()
        {
            return DateTime.Now.ToString().Replace("-", "_").Replace("/", "_").Replace(" ", "_").Replace(":", "_");
        }

        /// <summary>
        /// Return the path to a new unique image file.
        /// </summary>
        /// <returns>A new unique image file.</returns>
        public static string UniqueImageFileName(string baseDirectory)
        {
            return Path.Combine(baseDirectory, uniqueImageFileName());
        }

        /// <summary>
        /// Creates a unique file name for storing position data.
        /// </summary>
        /// <param name="baseDirectory">The directory the file will be placed in.</param>
        /// <returns>The full path to the file.</returns>
        public static string UniquePositionsFileName(string baseDirectory)
        {
            return Path.Combine(baseDirectory, "raw.csv");
        }

        /// <summary>
        /// Create a new unique image file name.
        /// </summary>
        /// <returns>A new unique image file name.</returns>
        private static string uniqueImageFileName()
        {
            fileCount++;
            return imageFilePrefix + fileCount.ToString() + imageFileSuffix;
        }

    }

    
}
