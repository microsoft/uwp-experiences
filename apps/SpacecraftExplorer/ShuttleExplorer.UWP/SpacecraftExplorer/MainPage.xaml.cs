using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using UnityPlayer;
using Windows.UI.Input;
using System.Diagnostics;

namespace SpacecraftExplorer
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
    {
        public enum DialMode { TimelineScrubMode, ExploreMode, Rotate, Zoom };
        DialMode campoMode = DialMode.TimelineScrubMode;

        private WinRTBridge.WinRTBridge _bridge;

        private float tooltipTime = 5.0f;
        private SplashScreen splash;
		private Rect splashImageRect;
		private WindowSizeChangedEventHandler onResizeHandler;
		private TypedEventHandler<DisplayInformation, object> onRotationChangedHandler;

        DispatcherTimer dispatcherTimer;
        DispatcherTimer updateSliderTimer;
        
        RadialController controller;
        RadialControllerMenuItem timelineMenuItem;
        RadialControllerMenuItem exploreMenuItem;
        RadialControllerMenuItem rotateMenuItem;
        RadialControllerMenuItem zoomMenuItem;

        bool hasSetInitialDialOption = false;

        public MainPage()
		{
			this.InitializeComponent();

            
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;

			AppCallbacks appCallbacks = AppCallbacks.Instance;
			// Setup scripting bridge
			_bridge = new WinRTBridge.WinRTBridge();
			appCallbacks.SetBridge(_bridge);

			appCallbacks.RenderingStarted += () => { RemoveSplashScreen(); };

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


            InitializeTimers();
            SetupSurfaceDial();

            
            this.Loaded += MainPage_Loaded;
        }

        private void TimelineMenuItem_Invoked(RadialControllerMenuItem sender, object args)
        {
            EnterSimulateMode();
            ExploreRadioButton.IsChecked = false;
            SimulateRadioButton.IsChecked = true;

            PlaceTooltip(RadialMenuCanvas.RenderSize.Width / 2.0f - TooltipControl.RenderSize.Width / 2.0f, 10.0f);
            TooltipControl.SetTooltip("Timeline Scrub", "Rewind", "Play from current", "Fast Forward");
            PlayTooltip();
            controller.UseAutomaticHapticFeedback = false;
            campoMode = DialMode.TimelineScrubMode;
        }


        private void ExploreMenuItem_Invoked(RadialControllerMenuItem sender, object args)
        {
            EnterInspectorMode();
            ExploreRadioButton.IsChecked = true;
            SimulateRadioButton.IsChecked = false;

            PlaceTooltip(RadialMenuCanvas.RenderSize.Width / 2.0f - TooltipControl.RenderSize.Width / 2.0f, 10.0f);
            TooltipControl.SetTooltip("Inspector", "Highlight previous component", "N/A", "Highlight next component");
            PlayTooltip();
            controller.UseAutomaticHapticFeedback = true;
            campoMode = DialMode.ExploreMode;
        }

        private void RotateMenuItem_Invoked(RadialControllerMenuItem sender, object args)
        {
            EnterInspectorMode();

            if (XRotTextBox.FocusState == FocusState.Unfocused && YRotTextBox.FocusState == FocusState.Unfocused && ZRotTextBox.FocusState == FocusState.Unfocused)
                XRotTextBox.Focus(FocusState.Programmatic);

            ExploreRadioButton.IsChecked = true;
            SimulateRadioButton.IsChecked = false;

            PlaceTooltip(RadialMenuCanvas.RenderSize.Width / 2.0f - TooltipControl.RenderSize.Width / 2.0f, 10.0f);
            TooltipControl.SetTooltip("Rotation tool", "Rotate counter clockwise", "Reset", "Rotate clockwise");
            PlayTooltip();
            controller.UseAutomaticHapticFeedback = false;
            campoMode = DialMode.Rotate;
        }

        private void ZoomMenuItem_Invoked(RadialControllerMenuItem sender, object args)
        {
            EnterInspectorMode();
            ExploreRadioButton.IsChecked = true;
            SimulateRadioButton.IsChecked = false;

            PlaceTooltip(RadialMenuCanvas.RenderSize.Width / 2.0f - TooltipControl.RenderSize.Width / 2.0f, 10.0f);
            TooltipControl.SetTooltip("Zoom", "Zoom out", "Reset zoom", "Zoom in");
            PlayTooltip();

            ZoomText.Focus(FocusState.Programmatic);

            controller.UseAutomaticHapticFeedback = false;
            campoMode = DialMode.Zoom;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Controller_ScreenContactContinued(RadialController sender, RadialControllerScreenContactContinuedEventArgs args)
        {
            PlaceDialMenu(args.Contact.Position.X, args.Contact.Position.Y);
        }

        private void Controller_ScreenContactEnded(RadialController sender, object args)
        {
            OnScreenRadialMenuControl.Visibility = Visibility.Collapsed;
        }

        private void Controller_ScreenContactStarted(RadialController sender, RadialControllerScreenContactStartedEventArgs args)
        {
            OnScreenRadialMenuControl.Visibility = Visibility.Visible;
            PlaceDialMenu(args.Contact.Position.X, args.Contact.Position.Y);
        }

        private void PlaceDialMenu(double x, double y)
        {
            OnScreenRadialMenuControl.SetValue(Canvas.LeftProperty, x - 512 / 2);
            OnScreenRadialMenuControl.SetValue(Canvas.TopProperty, y - 450 / 2 - 62);
        }
        private void PlaceTooltip(double x, double y)
        {
            TooltipControl.SetValue(Canvas.LeftProperty, x);
            TooltipControl.SetValue(Canvas.TopProperty, y);
        }

        bool _hasPlayedTooltip = false;
        private void DispatcherTimer_Tick(object sender, object e)
        {
            // Temp fix to set the default dial option after startup. Will remove when bug is fixed.
            if(!hasSetInitialDialOption)
            {
                controller.Menu.SelectMenuItem(timelineMenuItem);

                hasSetInitialDialOption = true;
            }

            double nt = ShuttleSimulator.currentNormalizedTime;
            if (nt > 1.0f)
                nt = 1.0f;
            TimeText3.Text = (nt * 225.0f).ToString("0");
            TimeText2.Text = (nt * 11000.0f).ToString("0");
            SpeedSlider.Value = (nt * 11000.0f);
            AltitudeSlider.Value = ((nt/1.2) * 225.0f);

            tooltipTime -= 1.0f;
            if (tooltipTime < 1.0f && !_hasPlayedTooltip)
            {
                _hasPlayedTooltip = true;
                HideTooltipGrid.Begin();
            }
        }

        private void Controller_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args)
        {
            if (campoMode == DialMode.TimelineScrubMode)
            {
                PlayAnimationFromSetTime();
            }

            if (campoMode == DialMode.ExploreMode)
            {
                ResetShuttleAttitude();
            }
            
            if (campoMode == DialMode.Zoom)
            {
                SetDefaultZoom();
            }
        }

        private void PlayAnimationFromSetTime()
        {
            ShuttleSimulator.ScrubAnimationMode(false);
            ShuttleSimulator.PlayAnimationFrom(ShuttleSimulator.animationTime);
            TimelineSlider.Value = ShuttleSimulator.currentNormalizedTime;
        }

        private void ResetShuttleAttitude()
        {
            ShuttleAttitudeControl.SetRotateX(0.0f);
            ShuttleAttitudeControl.SetRotateY(0.0f);
            ShuttleAttitudeControl.SetRotateZ(0.0f);


            XRotTextBox.Text = "0";
            YRotTextBox.Text = "0";
            ZRotTextBox.Text = "0";
        }

        private void SetDefaultZoom()
        {
            ShuttleExplorer.fov = 43.0f;
            ZoomText.Text = "65";
        }

        private void MoveFocusToNextTextBox()
        {
            if (!FocusManager.TryMoveFocus(FocusNavigationDirection.Right))
            {
                XRotTextBox.Focus(FocusState.Programmatic);
            }
        }

        private void ZRotTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox cTB = (TextBox)sender;
            float rotV = float.Parse(cTB.Text);
            ShuttleAttitudeControl.SetRotateZ(rotV);
        }

        private void YRotTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox cTB = (TextBox)sender;
            float rotV = float.Parse(cTB.Text);
            ShuttleAttitudeControl.SetRotateY(rotV);
        }

        private void XRotTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox cTB = (TextBox)sender;
            float rotV = float.Parse(cTB.Text);
            ShuttleAttitudeControl.SetRotateX(rotV);
        }

        double rotDelta = 0.0;
        private void Controller_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            OnScreenRadialMenuControl.Rotate(args.RotationDeltaInDegrees);
            RefreshOnscreenMenu();

            // if this is true, we only handle rotation changes in this round
            //bool changeAttitude = RefreshOnscreenMenu();

            //if (changeAttitude)
            //    return;

            if (campoMode == DialMode.TimelineScrubMode)
            {
                ShuttleSimulator.SetTime((float)args.RotationDeltaInDegrees * 0.01f);
                RefreshTimelineScrub();
            }

            if (campoMode == DialMode.ExploreMode)
            {
                rotDelta += args.RotationDeltaInDegrees;
                RefreshInspectionMode();
            }

            if (campoMode == DialMode.Zoom)
            {
                ShuttleExplorer.IncreaseZoom((float)args.RotationDeltaInDegrees * -1.0f);
                RefreshZoomMode();
            }
        }

        private bool RefreshOnscreenMenu()
        {
            bool changeAttitude = false;
            var fElmt = FocusManager.GetFocusedElement();
            if (fElmt != null)
            {
                TextBox fTb = fElmt as TextBox;
                if (fTb != null)
                {
                    string t = fTb.Text; double nr;
                    if (double.TryParse(t, out nr))
                    {
                        changeAttitude = true;
                        OnScreenRadialMenuControl.SetDisplayContent(nr.ToString("0.00"));
                        OnScreenRadialMenuControl.SetGenericDialMode();
                        controller.UseAutomaticHapticFeedback = false;

                        if (ZoomText.FocusState == FocusState.Unfocused)
                        {
                            // we are modifying a rotation field, set to rotation mode.
                            if (controller.Menu.GetSelectedMenuItem() != rotateMenuItem)
                                controller.Menu.SelectMenuItem(rotateMenuItem);
                        }
                        else
                        {
                            // we are modifying the zoom field, set to zoom mode.
                            if (controller.Menu.GetSelectedMenuItem() != zoomMenuItem)
                                controller.Menu.SelectMenuItem(zoomMenuItem);
                        }


                        if (ZoomText.FocusState == FocusState.Unfocused)
                            changeAttitude = false;
                    }
                }
            }

            return changeAttitude;
        }

        private void SimulateRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ExploreGrid == null || SimulateGrid == null)
                return;

            controller.Menu.SelectMenuItem(timelineMenuItem);

            EnterSimulateMode();

            campoMode = DialMode.TimelineScrubMode;
        }

        private void ExploreRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EnterInspectorMode();

            // Set to explore mode unless you are in rotate mode
            if(campoMode != DialMode.Rotate || campoMode != DialMode.Zoom)
                campoMode = DialMode.ExploreMode;
        }

        private void UpdateSliderTimer_Tick(object sender, object e)
        {
            TimelineSlider.Value = ShuttleSimulator.currentNormalizedTime;
            MissionAndComponentText.Text = ShuttleHelper.ComponentInfo(ShuttleExplorer.GetHighlightID());

            ShuttleSimulator.FireShuttleThrusters(false);
            ShuttleSimulator.FireSLBThrusters(false);

            if (ShuttleSimulator.currentNormalizedTime >= 0.1f)
            {
                ShuttleSimulator.FireShuttleThrusters(true);
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.13f)
            {
                ShuttleSimulator.FireShuttleThrusters(true);
                ShuttleSimulator.FireSLBThrusters(true);
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.55f)
            {
                ShuttleSimulator.FireShuttleThrusters(false);
                ShuttleSimulator.FireSLBThrusters(false);
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.78f)
            {
                ShuttleSimulator.FireShuttleThrusters(true);
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.90f)
            {
                ShuttleSimulator.FireShuttleThrusters(false);
                ShuttleSimulator.FireSLBThrusters(false);
            }
            MissionStageText.Text = ShuttleHelper.GetMissionStage();
        }



        private void InitializeTimers()
        {

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            updateSliderTimer = new DispatcherTimer();
            updateSliderTimer.Tick += UpdateSliderTimer_Tick;
            updateSliderTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);

            dispatcherTimer.Start();
            updateSliderTimer.Start();

        }

        private void SetupSurfaceDial()
        {
            // hooking up the events
            controller = RadialController.CreateForCurrentView();
            controller.RotationResolutionInDegrees = 1;
            controller.RotationChanged += Controller_RotationChanged;
            controller.ButtonClicked += Controller_ButtonClicked;
            controller.ScreenContactStarted += Controller_ScreenContactStarted;
            controller.ScreenContactEnded += Controller_ScreenContactEnded;
            controller.ScreenContactContinued += Controller_ScreenContactContinued;

            SurfaceDialStepTextboxHelper.Controller = controller;

            // If these textboxes has changes, the Unity Layer needs to know.
            XRotTextBox.TextChanged += XRotTextBox_TextChanged;
            YRotTextBox.TextChanged += YRotTextBox_TextChanged;
            ZRotTextBox.TextChanged += ZRotTextBox_TextChanged;

            // creating custom menu items
            var randomAccessStreamReference = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/Timeline-96x96.png", UriKind.RelativeOrAbsolute));
            timelineMenuItem = RadialControllerMenuItem.CreateFromIcon("Timeline", randomAccessStreamReference);
            timelineMenuItem.Invoked += TimelineMenuItem_Invoked;

            randomAccessStreamReference = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/Inspector-96x96.png", UriKind.RelativeOrAbsolute));
            exploreMenuItem = RadialControllerMenuItem.CreateFromIcon("Inspect", randomAccessStreamReference);
            exploreMenuItem.Invoked += ExploreMenuItem_Invoked;

            randomAccessStreamReference = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/3d-rotate-96x96.png", UriKind.RelativeOrAbsolute));
            rotateMenuItem = RadialControllerMenuItem.CreateFromIcon("Rotate", randomAccessStreamReference);
            rotateMenuItem.Invoked += RotateMenuItem_Invoked;

            zoomMenuItem = RadialControllerMenuItem.CreateFromKnownIcon("Zoom", RadialControllerMenuKnownIcon.Zoom);
            zoomMenuItem.Invoked += ZoomMenuItem_Invoked;

            // Remove excess default shell menu items
            RadialControllerConfiguration.GetForCurrentView().SetDefaultMenuItems(new[] { RadialControllerSystemMenuItemKind.Volume });

            // add all custom menu items
            controller.Menu.Items.Add(timelineMenuItem);
            controller.Menu.Items.Add(exploreMenuItem);
            controller.Menu.Items.Add(rotateMenuItem);
            controller.Menu.Items.Add(zoomMenuItem);


        }

        public void PlayTooltip()
        {
            tooltipTime = 5.0f;
            _hasPlayedTooltip = false;
            ShowTooltipGrid.Begin();
        }
        
        private void EnterSimulateMode()
        {

            if (ExploreGrid == null || SimulateGrid == null)
                return;

            ExploreGrid.Visibility = Visibility.Collapsed;
            SimulateGrid.Visibility = Visibility.Visible;


            ShuttleModeHandler.SetMode(ShuttleModeHandler.ShuttleMode.Simulate);
        }

        private void EnterInspectorMode()
        {

            if (ExploreGrid == null || SimulateGrid == null)
                return;

            ExploreGrid.Visibility = Visibility.Visible;
            SimulateGrid.Visibility = Visibility.Collapsed;

            ShuttleModeHandler.SetMode(ShuttleModeHandler.ShuttleMode.Explore);
        }

        private void RefreshZoomMode()
        {
            float zP = ShuttleExplorer.fov / 65.0f;

            ZoomText.Text = (zP * 100.0f).ToString("0.0");

            OnScreenRadialMenuControl.SetGenericDialMode();
            controller.UseAutomaticHapticFeedback = false;
        }

        private void RefreshInspectionMode()
        {
            if (rotDelta > 10)
            {
                rotDelta = 0.0f;
                ShuttleExplorer.IncreaseSelection();
            }
            else if (rotDelta < -10)
            {
                rotDelta = 0.0f;
                ShuttleExplorer.DecreaseSelection();
            }

            string hlC = ShuttleHelper.GetHighlightComponent();
            SelectionInfoTextBox.Text = hlC;

            XRotTextBox.Text = ShuttleAttitudeControl.RotX.ToString();
            YRotTextBox.Text = ShuttleAttitudeControl.RotY.ToString();
            ZRotTextBox.Text = ShuttleAttitudeControl.RotZ.ToString();

            OnScreenRadialMenuControl.SetDisplayContent(hlC);
            OnScreenRadialMenuControl.SetInspectorDialMode();
            controller.UseAutomaticHapticFeedback = true;
        }

        private void RefreshTimelineScrub()
        {
            ShuttleSimulator.ScrubAnimationMode(true);
            TimelineSlider.Value = ShuttleSimulator.animationTime;
            if (ShuttleSimulator.animationTime < 0.0f)
                ShuttleSimulator.animationTime = 0.0f;
            if (ShuttleSimulator.animationTime > 1.0f)
                ShuttleSimulator.animationTime = 1.0f;


            OnScreenRadialMenuControl.SetDisplayContent(ShuttleHelper.GetMissionStage());
            OnScreenRadialMenuControl.SetInspectorDialMode();
            controller.UseAutomaticHapticFeedback = false;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			splash = (SplashScreen)e.Parameter;
			OnResize();
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

				await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.High, delegate()
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
    }
}
