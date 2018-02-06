using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.SC.Models
{
    public class SuggestedTracks
    {
        public string href { get; set; }
    }

    public class Links
    {
        public SuggestedTracks suggested_tracks { get; set; }
    }

    public class Music
    {
        public string title { get; set; }
        public Links _links { get; set; }
    }

    public class SuggestedTracks2
    {
        public string href { get; set; }
    }

    public class Links2
    {
        public SuggestedTracks2 suggested_tracks { get; set; }
    }

    public class Audio
    {
        public string title { get; set; }
        public Links2 _links { get; set; }
    }

    public class PopularAudio
    {
        public string href { get; set; }
    }

    public class PopularMusic
    {
        public string href { get; set; }
    }

    public class Links3
    {
        public PopularAudio popular_audio { get; set; }
        public PopularMusic popular_music { get; set; }
    }

    public class Categories
    {
        public List<Music> music { get; set; }
        public List<Audio> audio { get; set; }
        public string tracking_tag { get; set; }
        public Links3 _links { get; set; }
    }
}
