using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UnityPlayer;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Atmosphere
{

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
	{
		private AppCallbacks appCallbacks;
		public SplashScreen splashScreen;

        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceconnection;

        static string deviceFamily;
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			appCallbacks = new AppCallbacks();

            Application.Current.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
        }

        public static bool IsXbox()
        {
            if (deviceFamily == null)
                deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            return deviceFamily == "Windows.Xbox";
        }

        /// <summary>
        /// Invoked when application is launched through protocol.
        /// Read more - http://msdn.microsoft.com/library/windows/apps/br224742
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
		{
			string appArgs = "";
			
			switch (args.Kind)
			{
				case ActivationKind.Protocol:
					ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
					splashScreen = eventArgs.SplashScreen;
					appArgs += string.Format("Uri={0}", eventArgs.Uri.AbsoluteUri);
					break;
			}

            InitializeUnity(appArgs);
		}

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            // Get a deferral so that the service isn't terminated.
            this.backgroundTaskDeferral = args.TaskInstance.GetDeferral();

            // Associate a cancellation handler with the background task.
            args.TaskInstance.Canceled += OnTaskCanceled; 

            // Retrieve the app service connection and set up a listener for incoming app service requests.
            var details = args.TaskInstance.TriggerDetails as AppServiceTriggerDetails;
            appServiceconnection = details.AppServiceConnection;
            appServiceconnection.RequestReceived += OnRequestReceived;
            appServiceconnection.ServiceClosed += AppServiceconnection_ServiceClosed;
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();

            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            if (message.ContainsKey("Command"))
            {
                string command = message["Command"] as string;
                // ... //
                if (command == "CurrentWeather")
                {
                    string resultJson = string.Empty;

                    var weatherData = await WeatherService.GetWeatherData();

                    var index = DateTime.Now.Hour / 6;

                    returnData.Add("Temperature", weatherData.Today[index].Temperature);
                    returnData.Add("State", weatherData.Today[index].Weather);
                    returnData.Add("Status", "OK");
                }
                else
                {
                    returnData.Add("Status", "Fail: Unknown command");
                }

            }
            else
            {
                returnData.Add("Status", "Fail: Missing command");
            }

            await args.Request.SendResponseAsync(returnData); // Return the data to the caller.
            messageDeferral.Complete(); // Complete the deferral so that the platform knows that we're done responding to the app service call.

        }

        private void AppServiceconnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            if (this.backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                this.backgroundTaskDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                this.backgroundTaskDeferral.Complete();
            }
        }

        /// <summary>
        /// Invoked when application is launched via file
        /// Read more - http://msdn.microsoft.com/library/windows/apps/br224742
        /// </summary>
        /// <param name="args"></param>
        protected override void OnFileActivated(FileActivatedEventArgs args)
		{
			string appArgs = "";

			splashScreen = args.SplashScreen;
			appArgs += "File=";
			bool firstFileAdded = false;
			foreach (var file in args.Files)
			{
				if (firstFileAdded) appArgs += ";";
				appArgs += file.Path;
				firstFileAdded = true;
			}

			InitializeUnity(appArgs);
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used when the application is launched to open a specific file, to display
		/// search results, and so forth.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonPressedForegroundColor = Colors.Black;
                }

                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 700));
            }

            var str = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            splashScreen = args.SplashScreen;
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            InitializeUnity(args.Arguments);
		}

		private void InitializeUnity(string args)
		{
#if UNITY_WP_8_1 || UNITY_UWP
			ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;
#if UNITY_UWP
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
#endif
#pragma warning disable 4014
			{
				StatusBar.GetForCurrentView().HideAsync();
			}
#pragma warning restore 4014
#endif
			appCallbacks.SetAppArguments(args);
			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null && !appCallbacks.IsInitialized())
			{
				rootFrame = new Frame();
				Window.Current.Content = rootFrame;
				Window.Current.Activate();

				rootFrame.Navigate(typeof(MainPage));
			}

			Window.Current.Activate();
		}
	}
}
