using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Newtonsoft.Json;

namespace VirginiaTech.Fuse.Util
{
    class TrainingWeights
    {
        private Dictionary<JointType, double> weightDict;

        public TrainingWeights(IDictionary<JointType, double> sourceWeightDict)
        {
            weightDict = new Dictionary<JointType, double>(sourceWeightDict);
        }

        private double getWeight(JointType jointType)
        {
            double weight;
            if (weightDict.TryGetValue(jointType, out weight))
            {
                return weight;
            }
            else
            {
                return 0;
            }
        }

        public bool matchData(JointPositions jointPositions)
        {
            //TODO Is there a way to generalize this? Maybe some kind of static method that can be used by training too?
            throw new Exception();
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", base.ToString(), JsonConvert.SerializeObject(weightDict));
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(weightDict);
        }

        public static TrainingWeights Parse(string text)
        {
            return new TrainingWeights(JsonConvert.DeserializeObject<Dictionary<JointType, double>>(text));
        }
    }
}
