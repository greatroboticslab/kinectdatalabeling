using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;


namespace VirginiaTech.Fuse.Record
{
    /// <summary>
    /// A controller for the angle of the Kinect sensor.
    /// </summary>
    public class KinectAngleAdjuster
    {
        private KinectSensor sensor;
        private int elevationAngleAdjust;

        /// <summary>
        /// Initialize a basic angle adjuster.
        /// </summary>
        /// <param name="sensor">A sensor with a motor.</param>
        public KinectAngleAdjuster(KinectSensor sensor)
        {
            this.sensor = sensor;
            elevationAngleAdjust = sensor.ElevationAngle;
        }

        /// <summary>
        /// Don't change the actual angle, but change the angle to change to be a little lower.
        /// </summary>
        public void AngleDown()
        {
            elevationAngleAdjust = Math.Max(elevationAngleAdjust - 1, sensor.MinElevationAngle);
        }

        /// <summary>
        /// Don't change the actual angle, but change the angle to change to be a little higher.
        /// </summary>
        public void AngleUp()
        {
            elevationAngleAdjust = Math.Min(elevationAngleAdjust + 1, sensor.MaxElevationAngle);
        }

        /// <summary>
        /// Update the actual angle to the current adjust angle.
        /// </summary>
        public void Execute()
        {
            sensor.ElevationAngle = elevationAngleAdjust;
        }

        /// <summary>
        /// View the current elevation angle.
        /// </summary>
        public int ElevationAngle { get { return sensor.ElevationAngle; } }

        /// <summary>
        /// The elevation angle a call to execute will change the sensor to.
        /// </summary>
        public int ElevationAdjustAngle
        {
            get { return elevationAngleAdjust; }
            set { elevationAngleAdjust = value; }
        }
    }
}
