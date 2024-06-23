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
    public class UndefinedJointPositionException : Exception
    {
        public readonly JointType badJointType;

        public UndefinedJointPositionException(JointType badJointType)
        {
            this.badJointType = badJointType;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class JointPositions
    {
        private IDictionary<JointType, Vector3> jointPositionDict;
        private string fileName;

        public JointPositions(string fileName, IDictionary<JointType, Vector3> sourceJointPositionDict)
        {
            this.fileName = fileName;
            jointPositionDict = new Dictionary<JointType, Vector3>(sourceJointPositionDict);
        }

        public string FileName { get { return fileName; } }

        public Vector3 this[JointType jointType]
        {
            get
            {
                Vector3 position;
                if (jointPositionDict.TryGetValue(jointType, out position))
                {
                    return position;
                }
                else
                {
                    throw new UndefinedJointPositionException(jointType);
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", base.ToString(), FileName, JsonConvert.SerializeObject(jointPositionDict));
        }
    }
}
