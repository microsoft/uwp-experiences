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
// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace SpacecraftExplorer
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		private AppCallbacks appCallbacks;
		public SplashScreen splashScreen;
		
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			SetupOrientation();
			appCallbacks = new AppCallbacks();
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
			splashScreen = args.SplashScreen;
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

		void SetupOrientation()
		{
#if UNITY_UWP
			Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Landscape|Windows.Graphics.Display.DisplayOrientations.LandscapeFlipped|Windows.Graphics.Display.DisplayOrientations.Portrait|Windows.Graphics.Display.DisplayOrientations.PortraitFlipped;
		ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
#endif
		}
	}
}
