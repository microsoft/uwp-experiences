using Microsoft.Toolkit.Uwp.Services.Twitter;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Music.PCL;
using Music.PCL.Models;
using Music.PCL.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Music
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootPage : Page
    {
        public RootPage()
        {
            this.InitializeComponent();
            this.Loaded += RootPage_Loaded;
        }

        private async void RootPage_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLoading("authenticating");
            if (App.Me == null)
            {
                try
                {
                    var success = await Authenticate();
                }
                catch
                {

                }
                
            }

            if (App.IsXbox())
            {
                this.Frame.Navigate(typeof(MainPage));
            }

            HideLoading();

            var optionCenterY = (float)optionsView.DesiredSize.Height / 2;
            var optionCenterX = (float)optionsView.DesiredSize.Width / 2;
            optionsView.Scale(0.8f, 0.8f, optionCenterX, optionCenterY, duration: 0)
                       .Then().Scale(1, 1, optionCenterX, optionCenterY).Fade(1).Start();

        }

        private async Task<bool> Authenticate()
        {
            var settings = ApplicationData.Current.LocalSettings;
            var userdata = settings.Values["user"];
            if (userdata != null)
            {
                var user = JsonConvert.DeserializeObject<User>(userdata as string);
                CloudService.Instance.Init(user);
                return true;
            }
            
            TwitterService.Instance.Initialize(Keys.TW_CONSUMERKEY, Keys.TW_CONSUMERSECRET, Keys.TW_CALLBACKURI);

            if (await TwitterService.Instance.LoginAsync())
            {
                var user = await TwitterService.Instance.GetUserAsync();

                var me = new User()
                {
                    Name = user.Name,
                    ProfileImage = user.ProfileImageUrl,
                    TwitterId = user.Id
                };

                settings.Values["user"] = JsonConvert.SerializeObject(me);

                CloudService.Instance.Init(me);

                // TODO - need to make sure it connects to the service
                return true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog($"You need to log in to Twitter first to continue!", "Log in!");
                dialog.Commands.Add(new UICommand("OK"));

                await dialog.ShowAsync();
                return await Authenticate();
            }
        }

        private async void JoinClicked(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            optionsView.IsHitTestVisible = false;
            var optionCenterY = (float)optionsView.DesiredSize.Height / 2;
            var optionCenterX = (float)optionsView.DesiredSize.Width / 2;
            optionsView.Scale(0.8f, 0.8f, optionCenterX, optionCenterY).Fade(0).Start();


            var joinCenterY = (float)joinSection.DesiredSize.Height / 2;
            var joinCenterX = (float)joinSection.DesiredSize.Width / 2;

            joinSection.IsHitTestVisible = true;
            await joinSection.Scale(1.2f, 1.2f, joinCenterX, joinCenterY, 0)
                       .Then().Scale(1, 1, joinCenterX, joinCenterY).Fade(1).StartAsync();
            CodeTextBox.Focus(FocusState.Keyboard);

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            (sender as Button).IsEnabled = true;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            optionsView.IsHitTestVisible = true;
            var optionCenterY = (float)optionsView.DesiredSize.Height / 2;
            var optionCenterX = (float)optionsView.DesiredSize.Width / 2;
            optionsView.Scale(1f, 1f, optionCenterX, optionCenterY).Fade(1).Start();


            var joinCenterY = (float)joinSection.DesiredSize.Height / 2;
            var joinCenterX = (float)joinSection.DesiredSize.Width / 2;

            joinSection.IsHitTestVisible = false;
            joinSection.Scale(1.2f, 1.2f, joinCenterX, joinCenterY)
                       .Fade(0).Start();
        }

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private bool processingJoin = false;
        private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            errorBox.Opacity = 0;
            if (e.Key == Windows.System.VirtualKey.Back)
                return;


            if (CodeTextBox.Text.Count() == 6)
            {
                // join party
                CodeTextBox.IsEnabled = false;
                ShowLoading("joining party");
                if (await DataService.Instance.InitDataServiceAsClient(CodeTextBox.Text))
                {
                    Frame.Navigate(typeof(MainPage), false);
                }
                else
                {
                    HideLoading();
                    CodeTextBox.IsEnabled = true;
                    CodeTextBox.Focus(FocusState.Keyboard);
                    errorBox.Opacity = 1;
                    errorBox.Offset(10).Then().Offset(-10).Then().Offset(10).Then().Offset(-10).Then().Offset(0).SetDurationForAll(30).Start();
                }
            }
            else if (CodeTextBox.Text.Count() > 6)
            {
                CodeTextBox.Text = CodeTextBox.Text.Substring(0, 6);
                e.Handled = true;
                CodeTextBox.Select(6, 0);
            }
        }

        bool isloading = false;

        private async void ShowLoading(string text)
        {
            LoadingText.Text = text;
            if (!isloading)
            {
                isloading = true;

                Loading.Visibility = Visibility.Visible;

                while (isloading)
                {
                    await LoadingImage.Rotate(20f, 50f, 0).Then().Rotate(-20f, 50f, 0).SetDelay(200).SetDurationForAll(1000).StartAsync();
                }
            }
        }

        private void HideLoading()
        {
            isloading = false;
            Loading.Visibility = Visibility.Collapsed;
        }
    }
}
