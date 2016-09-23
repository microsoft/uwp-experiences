using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.SC.Models
{
    public class Comment
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int track_id { get; set; }
        public int? timestamp { get; set; }
        public string body { get; set; }
        public string uri { get; set; }
        public SCUser user { get; set; }
    }
}
