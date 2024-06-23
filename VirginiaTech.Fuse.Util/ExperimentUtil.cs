using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.IO;
using VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Util
{
    public class ExperimentUtil
    {
        /// <summary>
        /// Get an axis type from the user via stdin.
        /// </summary>
        /// <returns>The selected axis type.</returns>
        public static Axis axisTypeInput()
        {
            Axis[] axisTypes = { Axis.X, Axis.Y, Axis.Z };
            for (int i = 0; i < axisTypes.Length; i++)
            {
                System.Console.WriteLine("[{0}]\t{1}", i, axisTypes[i]);
            }

            while (true)
            {
                try
                {
                    String line = Console.ReadLine();
                    int index = Convert.ToInt16(line);
                    if (index < axisTypes.Length && index >= 0)
                    {
                        System.Console.WriteLine("You have selected {0}.", axisTypes[index]);
                        return axisTypes[index];
                    }
                    else
                    {
                        throw new System.FormatException();
                    }
                }
                catch (System.FormatException)
                {
                    System.Console.WriteLine("You must enter an integer within {0} to {1}.", 0, axisTypes.Length - 1);
                }
            }
        }

        /// <summary>
        /// Get a joint type from the user via stdin.
        /// </summary>
        /// <returns>The selected joint type.</returns>
        public static JointType jointTypeInput()
        {
            List<JointType> jointTypes = new List<JointType>();
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
            {
                jointTypes.Add(jointType);
            }
            for (int i = 0; i < jointTypes.Count; i++)
            {
                System.Console.WriteLine("[{0}]\t{1}", i, jointTypes.ElementAt(i));
            }

            while (true)
            {
                try
                {
                    String line = Console.ReadLine();
                    int index = Convert.ToInt16(line);
                    if (index < jointTypes.Count)
                    {
                        JointType jointType = jointTypes.ElementAt(index);
                        System.Console.WriteLine("You have selected {0}.", jointType);
                        return jointType;
                    }
                    else
                    {
                        throw new FormatException();
                    }

                }
                catch (System.FormatException)
                {
                    System.Console.WriteLine("You must enter an integer within {0} to {1}.", 0, jointTypes.Count - 1);
                }
            }
        }

        /// <summary>
        /// Convert a skeleton point into a 3D vector.
        /// </summary>
        /// <param name="skeletonPoint">The skeleton point to convert.</param>
        /// <returns>A  new vector with the skeleton point data.</returns>
        public static Vector3 skeletonPointToVector(SkeletonPoint skeletonPoint)
        {
            return new Vector3(skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z);
        }

        /// <summary>
        /// Finds an open kinect sensor.
        /// </summary>
        /// <returns>A kinect sensor.</returns>
        public static KinectSensor connectToKinect()
        {
            while (true)
            {
                foreach (KinectSensor sensor in KinectSensor.KinectSensors)
                {
                    return sensor;
                }
            }
        }
    }
}
