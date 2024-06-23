using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Util = VirginiaTech.Fuse.Util;

namespace VirginiaTech.Fuse.Record
{

    public class PositionRecorder
    {
        private IList<Util.JointPositions> jointPositionsFrames;
        private bool recording;

        public PositionRecorder()
        {
            this.jointPositionsFrames = new List<Util.JointPositions>();
        }

        public void AddFrame(Util.JointPositions jointsPositions)
        {
            if (recording)
            {
                jointPositionsFrames.Add(jointsPositions);
            }
        }

        public void Save(string fileName)
        {
            using (StreamWriter writer = File.CreateText(fileName))
            {
                writer.Write(JsonConvert.SerializeObject(jointPositionsFrames, Formatting.Indented));
            }
        }

        public void Clear()
        {
            jointPositionsFrames.Clear();
        }

        public void SaveAndClear(string fileName)
        {
            Save(fileName);
            Clear();
        }

        public bool Recording { get { return recording; } set { recording = value; } }
    }
}
