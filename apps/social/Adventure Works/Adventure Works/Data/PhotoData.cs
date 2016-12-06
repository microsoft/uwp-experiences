using Adventure_Works.CognitiveServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure_Works.Data
{
    public class PhotoData
    {
        public Guid Id { get; set; }
        public IEnumerable<PhotoFace> People { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public DateTime DateTime { get; set; }
        public string Uri { get; set; }
        public string ThumbnailUri { get; set; }
        public string InkUri { get; set; }
        public bool IsProcessedForFaces { get; set; } = false;
        public bool IsAnalyzed { get; set; } = false;
        public bool IsLocal { get; set; } = true;
    }
}
