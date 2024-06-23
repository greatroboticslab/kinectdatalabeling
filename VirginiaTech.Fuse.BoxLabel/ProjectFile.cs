using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;

namespace VirginiaTech.Fuse.BoxLabel
{
    enum Label {
        Head,
        LeftLeg,
        RightLeg,
    }

    /// <summary>
    /// The label information for an entire frame.
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    class FrameLabel
    {
        private IList<RegionLabel> regionLabels;

        public FrameLabel(IList<RegionLabel> regionLabels)
        {
            this.regionLabels = regionLabels;
        }
    }

    /// <summary>
    /// A labeled region of a frame.
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    class RegionLabel
    {
        private Label label;
        private Rectangle region;

        public RegionLabel(Label label, Rectangle region)
        {
            this.label = label;
            this.region = region;
        }
    }
}
