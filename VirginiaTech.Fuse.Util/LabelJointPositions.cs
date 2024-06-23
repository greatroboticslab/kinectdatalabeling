using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Newtonsoft.Json;
using VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Util
{
    /// <summary>
    /// An exception to be thrown when a piece of JSON is parsed succesfully, but
    /// a required key has no value.
    /// </summary>
    public class MissingDataParseException : Exception
    {
        /// <summary>
        /// The text that was being parsed.
        /// </summary>
        public readonly string text;
        /// <summary>
        /// The key that had no value.
        /// </summary>
        public readonly string name;

        public MissingDataParseException(string text, string name)
        {
            this.text = text;
            this.name = name;
        }

        public override string ToString()
        {
            return String.Format("The text {0} could not be parsed because it lacked a value for {1} : {2}",
                text,
                name,
                base.ToString());
        }
    }

    /// <summary>
    /// Represents a list of joint positions that have been labelled ain
    /// some way
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    public class LabelJointPositions
    {
        private Label label;
        private JointPositions jointPositions;

        public LabelJointPositions(Label label, JointPositions jointPositions)
        {
            this.label = label;
            this.jointPositions = jointPositions;
        }

        public Vector3 this[JointType jointType]
        {
            get
            {
                return jointPositions[jointType];
            }
        }

        public Label Label
        {
            get { return label; }
            set { label = value; }
        }

        public string FileName
        {
            get { return jointPositions.FileName; }
        }

        public override string ToString()
        {
 	         return String.Format("{0} {1} {2}", base.ToString(), label.ToString(), jointPositions.ToString());
        }
    }
}
