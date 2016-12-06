using Adventure_Works.Data;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Adventure_Works.CognitiveServices.LUIS
{
    class LUISIntentHandlers
    {
        [IntentHandler(0.7, Name = "showmap")]
        public async Task<bool> HandleShowMap(LuisResult result, object context)
        {
            LUISIntentStatus usingIntentRouter = (LUISIntentStatus)context;

            var entity = result.Entities.Where(e => e.Key.StartsWith("builtin.geography")).Select(e => e.Value).FirstOrDefault();
            if (entity == null)
            {
                usingIntentRouter.Success = false;
                return false;
            }

            var location = entity.First().Value;
            var type = entity.First().Name.Split('.').Last();

            if (MainPage.Instance != null)
            {
                MainPage.Instance.MoveMap(location, type);
            }
            else
            {
                ((Frame)Window.Current.Content).Navigate(typeof(MainPage), $"showmap:{location}:{type}");
            }

            usingIntentRouter.Success = true;
            return true;
        }

        [IntentHandler(0.4, Name = "showuser")]
        public async Task<bool> HandleShowUser(LuisResult result, object context)
        {
            LUISIntentStatus usingIntentRouter = (LUISIntentStatus)context;

            var entity = result.Entities.Where(e => e.Key == "username").Select(e => e.Value).FirstOrDefault();
            if (entity == null)
            {
                usingIntentRouter.Success = false;
                return false;
            }

            var username = entity.First().Value;


            var user = await DataProvider.Instance.GetUserByName(username);
            if (user != null)
            {
                usingIntentRouter.TextResponse = result.OriginalQuery.Replace(username, user.Name);
                var adventure = await DataProvider.Instance.GetLatestAdventureByUser(user);
                ((Frame)Window.Current.Content).Navigate(typeof(AdventurePage), adventure.Id.ToString());
            }
            else
            {

            }

            usingIntentRouter.Success = true;
            return true;
        }

        [IntentHandler(0.4, Name = "whoisclosest")]
        public async Task<bool> HandleWhoIsClosest(LuisResult result, object context)
        {
            LUISIntentStatus usingIntentRouter = (LUISIntentStatus)context;

            var adventures = await DataProvider.Instance.GetFriendsAdventures();
            var myLocation = await Maps.GetCurrentLocationAsync();

            if (myLocation == null)
            {
                usingIntentRouter.TextResponse = usingIntentRouter.SpeechRespose = "Could not find your location";
                return true;
            }

            double minDistance = double.PositiveInfinity;
            Adventure adv = null;

            foreach (var adventure in adventures)
            {
                var distance = Maps.GetDistanceBetweenPoints(myLocation.Position, adventure.Location);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    adv = adventure;
                }
            }

            if (adv != null)
            {
                usingIntentRouter.TextResponse = usingIntentRouter.SpeechRespose = $"Closest adventure is {adv.Name} by {adv.User.Name}";
                if (MainPage.Instance != null)
                {
                    MainPage.Instance.MoveMap(adv);
                }
            }
            else
            {
                usingIntentRouter.TextResponse = usingIntentRouter.SpeechRespose = $"Couldn't find the closest adventure";
            }

            usingIntentRouter.Success = true;
            return true;
        }

        [IntentHandler(0.65, Name = "None")]
        public async Task<bool> HandleNone(LuisResult result, object context)
        {
            LUISIntentStatus usingIntentRouter = (LUISIntentStatus)context;
            usingIntentRouter.Success = false;
            return true;
        }
    }
}
