using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Adventure_Works
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private Type _sourcePageType;
        private NavigationMode _navigationMode;
        private object _parameter;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var data = e.Parameter as Dictionary<string, object>;
            if (data != null)
            {
                _sourcePageType = data["SourcePageType"] as Type;
                _navigationMode = (NavigationMode)data["NavigationMode"];
                _parameter = data["Parameter"];
            }
            else
            {
                _sourcePageType = typeof(MainPage);
                _navigationMode = NavigationMode.Forward;
            }

            if (Identity.Instance.SavedProvider != IdentityProvider.Default)
            {
                if (await Identity.Instance.LoginAsync(Identity.Instance.SavedProvider))
                {
                    (App.Current as App).LoginHandled();
                }
            }
        }


        private async void Facebook_Click(object sender, RoutedEventArgs e)
        {
            if (await Identity.Instance.LoginAsync(IdentityProvider.Facebook))
            {
                (App.Current as App).LoginHandled();
            }
        }

        private async void Twitter_Click(object sender, RoutedEventArgs e)
        {
            
            if (await Identity.Instance.LoginAsync(IdentityProvider.Twitter))
            {
                (App.Current as App).LoginHandled();
            }
        }

        private void Skip_Clicked(object sender, RoutedEventArgs e)
        {
            Identity.Instance.SetSkipLogin();
            (App.Current as App).LoginHandled();
        }
    }
}
