using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure_Works.CognitiveServices.LUIS
{
    public class LUISIntentStatus
    {
        public bool Success { get; set; }
        public string TextResponse { get; set; }
        public string SpeechRespose { get; set; }
    }
}
