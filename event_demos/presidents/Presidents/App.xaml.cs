using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.System.Profile;

namespace Presidents
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // TODO: 3.0 - Change the pointer mode to support selected mode rather than pointer mode
            //App.Current.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // TODO: 1.2 - Ensure we use ALL of the window space.  That means we have to make sure we follow the safe area of the screen!
            // ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // TODO: 5.1 - If we want a ten foot view, we probably want color safe colors as well.  This dictionary solves that for default styles.
            //if (App.IsTenFoot)
            //{
            //    // use TV colorsafe values
            //    this.Resources.MergedDictionaries.Add(new ResourceDictionary
            //    {
            //        Source = new Uri("ms-appx:///TvSafeColors.xaml")
            //    });
            //}

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar != null)
            {
                Color titleBarColor = (Color)App.Current.Resources["SystemChromeMediumColor"];
                titleBar.BackgroundColor = titleBarColor;
                titleBar.ButtonBackgroundColor = titleBarColor;
            }

            MainPage shell = Window.Current.Content as MainPage;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (shell == null)
            {
                // Create a AppShell to act as the navigation context and navigate to the first page
                shell = new MainPage();

                // Set the default language
                shell.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                shell.AppFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Should load state from previous run
                }
            }

            // Place our app shell in the current Window
            Window.Current.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                // When the navigation stack isn't restored, navigate to the first page
                // suppressing the initial entrance animation.
                shell.AppFrame.Navigate(typeof(AllPresidentsView), e.Arguments, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
            }

            // Ensure the current window is active
            Window.Current.Activate();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 200));
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // Save application state and stop any background activity
            deferral.Complete();
        }


        // TODO: 1.0 - Helper function to set the app into Ten foot mode. 
        // TODO: 4.1 - See styles.xaml for how to configure the nav menu to look more like an XBOX and use safe areas
        /// <summary>
        /// This function is used to configure the app to run in 10 foot mode, even while running on a PC.
        /// If you connect your machine to a large TV and want the same XBOX type experience in the app, this 
        /// is one way to achieve this.
        /// </summary>
        public static bool IsTenFootPC { get; private set; } = IsTenFoot;

        public static bool IsTenFoot
        {
            get
            {
                return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" || IsTenFootPC;
            }
            set
            {
                if (value != IsTenFootPC)
                {
                    IsTenFootPC = value;
                    TenFootModeChanged?.Invoke(null, null);
                }
            }
        }

        public static event EventHandler TenFootModeChanged;
    }
}
