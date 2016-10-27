using Microsoft.Toolkit.Uwp.Services.Facebook;
using Microsoft.Toolkit.Uwp.Services.Twitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using winsdkfb;

namespace Adventure_Works
{
    public class Identity
    {
        private bool _loggedIn = false;

        private static Identity _instance;

        public static Identity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Identity();
                }
                return _instance;
            }
        }

        private IdentityProvider _savedProvider = IdentityProvider.Default;
        public IdentityProvider SavedProvider
        {
            get
            {
                if (_savedProvider == IdentityProvider.Default)
                {
                    var provider = ApplicationData.Current.LocalSettings.Values["provider"];
                    if (provider != null)
                    {
                        _savedProvider = (IdentityProvider)Enum.Parse(typeof(IdentityProvider), provider as string);
                    }
                }
                return _savedProvider;
            }
            private set
            {
                ApplicationData.Current.LocalSettings.Values["provider"] = ((IdentityProvider)value).ToString();
            }
        }

        public async Task<bool> LoginAsync(IdentityProvider identityProvider)
        {
            if (identityProvider == IdentityProvider.Default)
            {
                identityProvider = SavedProvider == IdentityProvider.Default ? IdentityProvider.Facebook : SavedProvider;
            }
            bool success;

            try
            {
                if (identityProvider == IdentityProvider.Facebook)
                {
                    FacebookService.Instance.Initialize(Keys.FacebookAppId);
                    success = await FacebookService.Instance.LoginAsync();
                }
                else
                {
                    TwitterService.Instance.Initialize(Keys.TwitterConsumerKey, Keys.TwitterConsumerSecret, Keys.TwitterCallbackUri);
                    success = await TwitterService.Instance.LoginAsync();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("PLEASE POPULATE Keys.cs WITH THE FACEBOOK OR TWITTER API KEYS");
                success = false;
            }

            if (success)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("skip_login");
                SavedProvider = identityProvider;
            }

            _loggedIn = success;
            return success;
        }

        public async Task SharePhotoAsync(IdentityProvider identityProvider, IRandomAccessStreamWithContentType stream)
        {
            if (identityProvider == IdentityProvider.Default && SavedProvider != IdentityProvider.Default)
            {
                identityProvider = SavedProvider;
            }
            else if (identityProvider == IdentityProvider.Default)
            {
                return;
            }

            if (identityProvider == IdentityProvider.Facebook)
            {
                if (SavedProvider != IdentityProvider.Facebook || !_loggedIn)
                {
                    if (!await LoginAsync(IdentityProvider.Facebook))
                    {
                        return;
                    }
                }

                var success = await FacebookService.Instance.PostPictureToFeedAsync("Shared from Adventure Works", "my photo", stream);
            }
            else
            {
                if (SavedProvider != IdentityProvider.Twitter || !_loggedIn)
                {
                    if (!await LoginAsync(IdentityProvider.Twitter))
                    {
                        return;
                    }
                }

                var success = await TwitterService.Instance.TweetStatusAsync("Shared from Adventure Works", stream);
            }


        }

        public bool CheckIfLoginIsNeeded()
        {
            var skipLogin = ApplicationData.Current.LocalSettings.Values["skip_login"];
            return _loggedIn || (skipLogin != null && (bool)skipLogin);
        }

        public void SetSkipLogin()
        {
            ApplicationData.Current.LocalSettings.Values["skip_login"] = true;
        }
    }

    public enum IdentityProvider
    {
        Default,
        Facebook,
        Twitter,
    }
}
