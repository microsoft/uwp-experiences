using Music.SC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.SC
{
    public class Next
    {
        public string href { get; set; }
    }

    public class Links2
    {
        public Next next { get; set; }
    }

    public class Waveform
    {
        public int width { get; set; }
        public int height { get; set; }
        public List<int> samples { get; set; }
    }

    public class Stream
    {
        public List<Track> collection { get; set; }
        public string tracking_tag { get; set; }
        public Links2 _links { get; set; }
    }
}
