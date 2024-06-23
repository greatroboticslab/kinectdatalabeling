using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirginiaTech.Fuse.Util
{
    public class Feature
    {
        private Label label;
        private double[] data;

        public Feature(Label label, double[] data)
        {
            this.label = label;
            this.data = data;
        }

        public Label Label { get { return label; } }
        public double[] Data { get { return data; } }
    }
}
