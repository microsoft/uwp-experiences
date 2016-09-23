using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.SC.Models
{
    public class Playlist
    {
        public int duration { get; set; }
        public object release_day { get; set; }
        public string permalink_url { get; set; }
        public object genre { get; set; }
        public string permalink { get; set; }
        public object purchase_url { get; set; }
        public object release_month { get; set; }
        public object description { get; set; }
        public string uri { get; set; }
        public object label_name { get; set; }
        public string tag_list { get; set; }
        public object release_year { get; set; }
        public int track_count { get; set; }
        public int user_id { get; set; }
        public string last_modified { get; set; }
        public string license { get; set; }
        public List<Track> tracks { get; set; }
        public object playlist_type { get; set; }
        public int id { get; set; }
        public bool downloadable { get; set; }
        public string sharing { get; set; }
        public string created_at { get; set; }
        public object release { get; set; }
        public string kind { get; set; }
        public string title { get; set; }
        public object type { get; set; }
        public object purchase_title { get; set; }
        public CreatedWith created_with { get; set; }
        public object artwork_url { get; set; }
        public object ean { get; set; }
        public bool streamable { get; set; }
        public SCUser user { get; set; }
        public string embeddable_by { get; set; }
        public object label_id { get; set; }
    }
}
