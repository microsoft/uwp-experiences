using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure_Works.CognitiveServices
{
    public class PhotoFace
    {
        public string FaceId { get; set; }
        public Guid PersonId { get; set; }
        public FaceRectangle Rect { get; set; }

        public string Name { get; set; }
        public bool Identified { get; set; }
    }
}
