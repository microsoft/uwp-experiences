using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.PCL.Models
{
    public class PartyStatus
    {
        public int TrackIndex { get; set; }
        public int Duration { get; set; }
        public int Progress { get; set; }
        public PlaybackState State { get; set; }
    }

    public enum PlaybackState
    {
        Playing,
        Paused,
        Other
    }
}
