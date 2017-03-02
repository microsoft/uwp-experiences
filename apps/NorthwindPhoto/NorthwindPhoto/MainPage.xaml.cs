// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Services.Twitter;
using NorthwindPhoto.Model;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NorthwindPhoto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if ((key == VirtualKey.Home) ||
                (key == VirtualKey.Back))
            {
                if (MainFrame.CanGoBack)
                    MainFrame.GoBack();
            }
        }

        /// <summary>
        /// TODO: 2. Pin to Start
        /// </summary>
        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            // Find the app in the current package
            var appListEntry = (await Package.Current.GetAppListEntriesAsync()).FirstOrDefault();

            if (appListEntry != null)
                await StartScreenManager.GetDefault().RequestAddAppListEntryAsync(appListEntry);
        }

        #region Navigation Setup

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.MainFrame = MainFrame;
            App.MainFrame.Navigated += MainFrame_Navigated;
            KeyUp += MainPage_KeyUp;

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, eventArgs) =>
            {
                if (MainFrame.CanGoBack)
                    MainFrame.GoBack();
            };

            App.MainFrame.Navigate(typeof(Gallery));
        }

        private async void MainPage_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.F3)
            {
                TwitterService.Instance.Initialize(Constants.ConsumerKey, Constants.ConsumerSecret, Constants.Callback);
                TwitterService.Instance.Logout();

                // Login to Twitter
                if (!await TwitterService.Instance.LoginAsync())
                    return;

                await new MessageDialog("Twitter Authenticated").ShowAsync();
            }
            else if (e.Key == VirtualKey.P)
            {
                App.ProtocolSubject.OnNext(new ProtocolDataEventArgs {Image = "all"});
            }
            else if (e.Key == VirtualKey.L)
            {
                App.ProtocolSubject.OnNext(new ProtocolDataEventArgs {Image = App.PhotoCollection.ElementAt(1).Path});
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var frame = sender as Frame;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = frame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;

            if (frameworkElement != null)
                switch (frameworkElement.Tag.ToString())
                {
                    case "Gallery":
                        if (App.MainFrame.Content is Gallery)
                            App.MainFrame.Navigate(typeof(Gallery), 1500);
                        else
                            App.MainFrame.Navigate(typeof(Gallery));
                        break;
                    case "Collage":
                        App.MainFrame.Navigate(typeof(Collage));
                        break;
                    default:
                        App.MainFrame.Navigate(typeof(Gallery));
                        break;
                }
        }

        #endregion
    }
}