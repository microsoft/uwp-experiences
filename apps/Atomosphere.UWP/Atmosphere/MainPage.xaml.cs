using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Windows;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using UnityPlayer;
using File = WinRTLegacy.Partial.System.IO.File;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.AppExtensions;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.System.Profile;

namespace Atmosphere
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private WinRTBridge.WinRTBridge _bridge;

        private SplashScreen splash;
        private Rect splashImageRect;
        private WindowSizeChangedEventHandler onResizeHandler;
        private TypedEventHandler<DisplayInformation, object> onRotationChangedHandler;
        DispatcherTimer _timer;

        private AppExtensionCatalog _catalog;
        private IEnumerable<StorageFile> _soundFiles;

        private ObservableCollection<Today> _todaysWeather;
        private ObservableCollection<WeekDay> _weekDays;

        public ObservableCollection<WeekDay> WeekDays
        {
            get { return _weekDays; }
            set
            {
                _weekDays = value;
                OnPropertyChanged();
            }

        }

        private DateTime _now = DateTime.Now;

        public DateTime Now
        {
            get { return _now; }
            set
            {
                _now = value;
                OnPropertyChanged();
            }
        }


        private void UpdateSize()
        {
            try
            {
                if (Window.Current.Bounds.Width >= 960 || App.IsXbox())
                {
                    WeatherController.SetOrientation(WeatherController.ViewOrientation.Landscape);
                    VisualStateManager.GoToState(this, "WideState", false);
                    return;
                }
                else
                {
                    WeatherController.SetOrientation(WeatherController.ViewOrientation.Portrait);
                    VisualStateManager.GoToState(this, "NarrowState", false);
                    return;
                }
            }
            catch { }
        }

        public ObservableCollection<Today> TodaysWeather
        {
            get { return _todaysWeather; }
            set
            {
                _todaysWeather = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            _catalog = AppExtensionCatalog.Open("Atmosphere");
            _catalog.PackageInstalled += _catalog_PackageInstalled;
            InstallPackageContent();

            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;

            AppCallbacks appCallbacks = AppCallbacks.Instance;
            // Setup scripting bridge
            _bridge = new WinRTBridge.WinRTBridge();
            appCallbacks.SetBridge(_bridge);

            appCallbacks.RenderingStarted += () => { RemoveSplashScreen(); };

            this.Loaded += MainPage_Loaded;
            this.KeyDown += MainPage_KeyDown;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += _timer_Tick;
            _timer.Start();

#if !UNITY_WP_8_1
            appCallbacks.SetKeyboardTriggerControl(this);
#endif
            appCallbacks.SetSwapChainPanel(GetSwapChainPanel());
            appCallbacks.SetCoreWindowEvents(Window.Current.CoreWindow);
            appCallbacks.InitializeD3DXAML();

            splash = ((App)App.Current).splashScreen;
            GetSplashBackgroundColor();
            OnResize();
            onResizeHandler = new WindowSizeChangedEventHandler((o, e) => OnResize());
            Window.Current.SizeChanged += onResizeHandler;

#if UNITY_WP_8_1
			onRotationChangedHandler = new TypedEventHandler<DisplayInformation, object>((di, o) => { OnRotate(di); });
			ExtendedSplashImage.RenderTransformOrigin = new Point(0.5, 0.5);
			var displayInfo = DisplayInformation.GetForCurrentView();
			displayInfo.OrientationChanged += onRotationChangedHandler;
			OnRotate(displayInfo);

			SetupLocationService();
#endif
        }

        private void _timer_Tick(object sender, object e)
        {
            Now = DateTime.Now;
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!App.IsXbox())
                e.Handled = true;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += (s, args) => {
                UpdateSize();
            };
            UpdateSize();

            
        }

        private void _catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
            InstallPackageContent();
            PlaySoundFile();
        }

        private async void InstallPackageContent()
        {
            foreach (var extension in await _catalog.FindAllAsync())
            {
                var folder = await extension.GetPublicFolderAsync();
                var files = await folder.GetFilesAsync();

                _soundFiles = files.Where(i => i.Name.EndsWith(".mp3"));
            }
        }

        private void PlaySoundFile()
        {
            var availableSoundFile = _soundFiles?.FirstOrDefault(i => i.Name.ToLower().Contains(TodaysWeather.First()?.Weather.ToLower()));

            if (availableSoundFile != null)
            {
                AtmospherePlayer.Source = MediaSource.CreateFromStorageFile(availableSoundFile);
                MusicSymbol.Visibility = Visibility;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            splash = (SplashScreen)e.Parameter;
            OnResize();

            var weatherData = await WeatherService.GetWeatherData();

            WeekDays = new ObservableCollection<WeekDay>(weatherData.WeekDay);
            TodaysWeather = new ObservableCollection<Today>(weatherData.Today);
            NowButton.IsChecked = true;
            WeatherController.SetView(WeatherController.ViewMode.TodayCloseUp);
            WeatherController.AssetsLoaded += WeatherController_AssetsLoaded;
        }

        private int NowTemperatureExtract(IEnumerable<Today> today)
        {
            int index = DateTime.Now.Hour / 6;

            if (today != null && today.Count() > index)
                return today.ElementAt(index).Temperature;
            return 0;
        }

        private bool _assetLoaded;
        private void WeatherController_AssetsLoaded(object sender, EventArgs e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // We use defer loading of the UI until Unity is ready and then we realize the XAML elements using x:DeferLoadStrategy="Lazy"
                FindName("WeatherPivot");
                PlaySoundFile();
                _assetLoaded = true;
            });
        }

        private void OnResize()
        {
            if (splash != null)
            {
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
        }

#if UNITY_WP_8_1
		private void OnRotate(DisplayInformation di)
		{
			// system splash screen doesn't rotate, so keep extended one rotated in the same manner all the time
			int angle = 0;
			switch (di.CurrentOrientation)
			{
			case DisplayOrientations.Landscape:
				angle = -90;
				break;
			case DisplayOrientations.LandscapeFlipped:
				angle = 90;
				break;
			case DisplayOrientations.Portrait:
				angle = 0;
				break;
			case DisplayOrientations.PortraitFlipped:
				angle = 180;
				break;
			}
			var rotate = new RotateTransform();
			rotate.Angle = angle;
			ExtendedSplashImage.RenderTransform = rotate;
		}
#endif

        private void PositionImage()
        {
            var inverseScaleX = 1.0f;
            var inverseScaleY = 1.0f;
#if UNITY_WP_8_1
			inverseScaleX = inverseScaleX / DXSwapChainPanel.CompositionScaleX;
			inverseScaleY = inverseScaleY / DXSwapChainPanel.CompositionScaleY;
#endif

            ExtendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X * inverseScaleX);
            ExtendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y * inverseScaleY);
            ExtendedSplashImage.Height = splashImageRect.Height * inverseScaleY;
            ExtendedSplashImage.Width = splashImageRect.Width * inverseScaleX;
        }

        private async void GetSplashBackgroundColor()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AppxManifest.xml"));
                string manifest = await FileIO.ReadTextAsync(file);
                int idx = manifest.IndexOf("SplashScreen");
                manifest = manifest.Substring(idx);
                idx = manifest.IndexOf("BackgroundColor");
                if (idx < 0)  // background is optional
                    return;
                manifest = manifest.Substring(idx);
                idx = manifest.IndexOf("\"");
                manifest = manifest.Substring(idx + 1);
                idx = manifest.IndexOf("\"");
                manifest = manifest.Substring(0, idx);
                int value = 0;
                bool transparent = false;
                if (manifest.Equals("transparent"))
                    transparent = true;
                else if (manifest[0] == '#') // color value starts with #
                    value = Convert.ToInt32(manifest.Substring(1), 16) & 0x00FFFFFF;
                else
                    return; // at this point the value is 'red', 'blue' or similar, Unity does not set such, so it's up to user to fix here as well
                byte r = (byte)(value >> 16);
                byte g = (byte)((value & 0x0000FF00) >> 8);
                byte b = (byte)(value & 0x000000FF);

                await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.High, delegate ()
                {
                    byte a = (byte)(transparent ? 0x00 : 0xFF);
                    ExtendedSplashGrid.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
                });
            }
            catch (Exception)
            { }
        }

        public SwapChainPanel GetSwapChainPanel()
        {
            return DXSwapChainPanel;
        }

        public void RemoveSplashScreen()
        {
            DXSwapChainPanel.Children.Remove(ExtendedSplashGrid);
            if (onResizeHandler != null)
            {
                Window.Current.SizeChanged -= onResizeHandler;
                onResizeHandler = null;

#if UNITY_WP_8_1
				DisplayInformation.GetForCurrentView().OrientationChanged -= onRotationChangedHandler;
				onRotationChangedHandler = null;
#endif
            }
        }

#if !UNITY_WP_8_1
        protected override Windows.UI.Xaml.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new UnityPlayer.XamlPageAutomationPeer(this);
        }
#else
		// This is the default setup to show location consent message box to the user
		// You can customize it to your needs, but do not remove it completely if your application
		// uses location services, as it is a requirement in Windows Store certification process
		private async void SetupLocationService()
		{
			AppCallbacks appCallbacks = AppCallbacks.Instance;
			if (!appCallbacks.IsLocationCapabilitySet())
			{
				return;
			}

			const string settingName = "LocationContent";
			bool userGaveConsent = false;

			object consent;
			var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
			var userWasAskedBefore = settings.Values.TryGetValue(settingName, out consent);

			if (!userWasAskedBefore)
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("Can this application use your location?", "Location services");

				var acceptCommand = new Windows.UI.Popups.UICommand("Yes");
				var declineCommand = new Windows.UI.Popups.UICommand("No");

				messageDialog.Commands.Add(acceptCommand);
				messageDialog.Commands.Add(declineCommand);

				userGaveConsent = (await messageDialog.ShowAsync()) == acceptCommand;
				settings.Values.Add(settingName, userGaveConsent);
			}
			else
			{
				userGaveConsent = (bool)consent;
			}

			if (userGaveConsent)
			{	// Must be called from UI thread
				appCallbacks.SetupGeolocator();
			}
		}
#endif
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuItem_Selected(object sender, RoutedEventArgs e)
        {
            if (!_assetLoaded) return;

            var radioButton = sender as RadioButton;

            if (radioButton?.Tag == null) return;

            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                WeatherController.SetOrientation(WeatherController.ViewOrientation.Portrait);
            }

            switch (radioButton.Tag.ToString())
            {
                case "Now":
                    WeatherPivot.SelectedIndex = 0;
                    WeatherController.SetView(WeatherController.ViewMode.TodayCloseUp);
                    break;
                case "Today":
                    WeatherPivot.SelectedIndex = 1;
                    WeatherController.SetView(WeatherController.ViewMode.TodayList);
                    break;
                case "Week":
                    WeatherPivot.SelectedIndex = 2;
                    WeatherController.SetView(WeatherController.ViewMode.FiveDayForecast);
                    break;
            }
        }

        private void WeatherPivot_PivotItemLoaded(Pivot sender, PivotItemEventArgs args)
        {
            UpdateSize();
        }

        private void ButtonGotFocus(object sender, RoutedEventArgs e)
        {
            (sender as RadioButton).IsChecked = true;
        }
    }
}
