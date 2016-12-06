using Adventure_Works.Rome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Adventure_Works
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        static string deviceFamily;

        public event EventHandler<BackRequestedEventArgs> BackRequested;

        private BackgroundTaskDeferral _backgroundTaskDeferral;
        private AppServiceConnection _appServiceconnection;

        Frame oldFrame;

        public App()
        {
            this.LeavingBackground += App_LeavingBackground;
            this.EnteredBackground += App_EnteredBackground;
            this.UnhandledException += App_UnhandledException;
            this.InitializeComponent();

            App.Current.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
        }

        public static bool IsXbox()
        {
            if (deviceFamily == null)
                deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            return deviceFamily == "Windows.Xbox";
        }

        public static bool IsMobile()
        {
            if (deviceFamily == null)
                deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            return deviceFamily == "Windows.Mobile";
        }

        private Frame HandleCommonActivation(IActivatedEventArgs e)
        {
            if (App.IsXbox())
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {

                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.HideAsync();
                }
            }

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = false;

                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {

                    var brandColor = (App.Current.Resources["BrandColor"] as SolidColorBrush).Color;
                    var brandColorLight = (App.Current.Resources["BrandColorLight"] as SolidColorBrush).Color;

                    titleBar.ButtonBackgroundColor = brandColor;
                    titleBar.ButtonInactiveBackgroundColor = brandColorLight;

                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonInactiveForegroundColor = Colors.Black;

                    titleBar.BackgroundColor = brandColor;
                    titleBar.InactiveBackgroundColor = (App.Current.Resources["BrandColorLight"] as SolidColorBrush).Color;
                }
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.Navigated += OnNavigated;

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                rootFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;

            Speech.SpeechService.Instance.Initialize();

            return rootFrame;

        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = HandleCommonActivation(e);

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            bool slideshow = false;
            string adventureId = null;

            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;

                var uri = eventArgs.Uri.AbsolutePath;
                var linkData = uri.Split('/');

                if (linkData.Count() > 1 && linkData[0] == "slideshow")
                {
                    slideshow = true;
                    adventureId = linkData[1];
                }
            }

            var rootFrame = HandleCommonActivation(args);

            if (slideshow)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.SetNavigationState("1,1,0,24,Adventure_Works.MainPage,12,0,0");
                }
                rootFrame.Navigate(typeof(SlideshowPage), adventureId);
            }
            else if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            BackRequested?.Invoke(sender, e);

            if (!e.Handled)
            {
                if (rootFrame != null && rootFrame.CanGoBack)
                {
                    e.Handled = true;
                    rootFrame.GoBack();

                }
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;

            if (!Identity.Instance.CheckIfLoginIsNeeded())
            {
                oldFrame = Window.Current.Content as Frame;

                var loginFrame = new Frame();
                Window.Current.Content = loginFrame;

                loginFrame.Navigate(typeof(LoginPage));
            }
        }

        public void LoginHandled()
        {
            Window.Current.Content = null;

            if (oldFrame != null)
            {
                Window.Current.Content = oldFrame;
            }
        }

        // Activated when connected with app service
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            this._backgroundTaskDeferral = args.TaskInstance.GetDeferral();
            args.TaskInstance.Canceled += OnTaskCanceled;

            var details = args.TaskInstance.TriggerDetails as AppServiceTriggerDetails;
            _appServiceconnection = details.AppServiceConnection;
            _appServiceconnection.RequestReceived += OnRequestReceived;
            _appServiceconnection.ServiceClosed += AppServiceconnection_ServiceClosed;

        }

        private void AppServiceconnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            if (_backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                _backgroundTaskDeferral.Complete();
            }
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            AppServiceDeferral messageDeferral = args.GetDeferral();

            var message = await ConnectedService.Instance.HandleMessageReceivedAsync(args.Request.Message, null, _appServiceconnection);
            await args.Request.SendResponseAsync(message);

            messageDeferral.Complete();
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (_backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                _backgroundTaskDeferral.Complete();
                _backgroundTaskDeferral = null;
            }
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            ConnectedService.Instance.PrepareForBackground();
        }

        private void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            ConnectedService.Instance.PrepareForForeground();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}
