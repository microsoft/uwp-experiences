using Adventure_Works.CognitiveServices.LUIS;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure_Works.CognitiveServices
{
    public class LUISAPI
    {
        private static LUISAPI _instance;
        public static LUISAPI Instance => _instance ?? (_instance = new LUISAPI());

        private IntentRouter _router;

        private LUISAPI() { }

        public async Task<LUISIntentStatus> HandleIntent(string text)
        {
            try
            {
                if (_router == null)
                {
                    var handlers = new LUISIntentHandlers();
                    _router = IntentRouter.Setup(Keys.LUISAppId, Keys.LUISAzureSubscriptionKey, handlers, false);
                }
                var status = new LUISIntentStatus();
                var handled = await _router.Route(text, status);

                return status;
            }
            catch(Exception ex)
            {
                return new LUISIntentStatus()
                {
                    SpeechRespose = "LUIS and I are not talking right now, make sure IDs are correct in Keys.cs",
                    TextResponse = "Can't access LUIS, make sure to populate Keys.cs with the LUIS IDs",
                    Success = true
                };
            }
        }

        
    }
}
