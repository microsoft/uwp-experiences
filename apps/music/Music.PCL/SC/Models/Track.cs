using System;
using System.Collections.Generic;
using System.Text;

namespace Music.SC.Models
{
    
    public class CreatedWith
    {
        public int id { get; set; }
        public string kind { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string external_url { get; set; }
    }

    public class Track
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int duration { get; set; }
        public bool commentable { get; set; }
        public string state { get; set; }
        public int original_content_size { get; set; }
        public string last_modified { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public bool streamable { get; set; }
        public string embeddable_by { get; set; }
        public bool downloadable { get; set; }
        public string purchase_url { get; set; }
        public object label_id { get; set; }
        public object purchase_title { get; set; }
        public string genre { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string label_name { get; set; }
        public string release { get; set; }
        public string track_type { get; set; }
        public string key_signature { get; set; }
        public string isrc { get; set; }
        public string video_url { get; set; }
        public double? bpm { get; set; }
        public object release_year { get; set; }
        public object release_month { get; set; }
        public object release_day { get; set; }
        public string original_format { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public SCUser user { get; set; }
        public string permalink_url { get; set; }
        public string artwork_url { get; set; }
        public string waveform_url { get; set; }
        public string stream_url { get; set; }
        public string download_url { get; set; }
        public int playback_count { get; set; }
        public int download_count { get; set; }
        public int favoritings_count { get; set; }
        public int comment_count { get; set; }
        public string attachments_uri { get; set; }
        public string policy { get; set; }
        public CreatedWith created_with { get; set; }


        public bool monetizable { get; set; }
        public Embedded _embedded { get; set; }
        public List<object> user_tags { get; set; }
        public string urn { get; set; }
        public LinksTemp _links { get; set; }

        public Track() { }

    }
    
    public class Stats
    {
        public int playback_count { get; set; }
        public int likes_count { get; set; }
        public int reposts_count { get; set; }
        public int comments_count { get; set; }
    }

    public class Embedded
    {
        public SCUser user { get; set; }
        public Stats stats { get; set; }
    }

    public class Related
    {
        public string href { get; set; }
    }

    public class LinksTemp
    {
        public Related related { get; set; }
    }
}
