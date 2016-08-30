using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Automation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.System.Profile;

namespace Presidents
{
    public sealed partial class MainPage : Page
    {

        #region Fields
        CoreCursor _defaultCursor;
        private Border visibleBounds;

        public static MainPage Current = null;

        public Frame AppFrame { get { return this.Frame; } }

        // Declare the top level nav items
        private List<NavMenuItem> navlist = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    Symbol = (Symbol) 0xec08, // courthouse
                    Label = "All Presidents",
                    DestPage = typeof(AllPresidentsView)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Favorite,
                    Label = "Democrats",
                    DestPage = typeof(AllPresidentsView),
                    Arguments = President.AllPresidents.Where(p => p.Party.StartsWith("Democrat")),
                },
                new NavMenuItem()
                {
                    Symbol = (Symbol) 0xe734,
                    Label = "Republicans",
                    DestPage = typeof(AllPresidentsView),
                    Arguments = President.AllPresidents.Where(p => p.Party.StartsWith("Republican")),
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Camera,
                    Label = "Photos",
                    DestPage = typeof(CompareView)
                },
            });

        #endregion

        /// <summary>
        /// Initializes a new instance of the AppShell, sets the static 'Current' reference,
        /// adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
        /// provide the nav menu list with the data to display.
        public MainPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            NavMenuList.ItemsSource = navlist;

            this.KeyDown += MainPage_KeyDown;

            _defaultCursor = CoreWindow.GetForCurrentThread().PointerCursor;

            UpdateTenFootMode();

            if (App.IsTenFoot) // Make sure the side panel is closed when in ten foot mode
            {
                SplitView_PaneClosed(null, null);
            }

            // Debugging helper!
            // NOTE: You don't really need this anymore with Visual Studio 2015 Update 3 
            // TODO: 3.2 - Debugging Visual State selection can be hard
            //this.GotFocus += (object sender, RoutedEventArgs e) =>
            //{
            //    FrameworkElement focus = FocusManager.GetFocusedElement() as FrameworkElement;
            //    if (focus != null)
            //        Debug.WriteLine("got focus: " + focus.Name + " (" + focus.GetType().ToString() + ")");
            //};
        }

        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            NavPaneDivider.Visibility = Visibility.Collapsed;

            // TODO: 3.3 - Cleanup these controls so they are not visible in the visual tree and won't become tab stops
            //SettingsButton.Visibility = Visibility.Collapsed;
            //TenFootModeToggle.Visibility = Visibility.Collapsed;
            //FeedbackButton.Visibility = Visibility.Collapsed;
        }

        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            NavPaneDivider.Visibility = Visibility.Visible;

            // TODO: 3.3 - Make sure they are added back so the controls DO work when the pane is expanded
            //SettingsButton.Visibility = Visibility.Visible;
            //TenFootModeToggle.Visibility = Visibility.Visible;
            //FeedbackButton.Visibility = Visibility.Visible;
        }


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += MainPage_KeyDown;

            Current = this;

            this.NavMenuList.SelectedIndex = 0;

            // Check if we are on XBOX, hide the 10 Foot Mode button (can only run full screen so we don't need this button)
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
                TenFootModeToggle.Visibility = Visibility.Collapsed;
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // TODO: 3.1 - Check for the gamepad, did the user hit the view button?
            //if (e.OriginalKey == VirtualKey.GamepadView)
            //{
            //    TogglePaneButton.IsChecked = !(bool)(TogglePaneButton.IsChecked);
            //    TogglePaneButton.Focus(FocusState.Keyboard);

            //    if (App.IsTenFoot)
            //        ElementSoundPlayer.Play(ElementSoundKind.Invoke);

            //    e.Handled = true;
            //}

            // TODO: Nice trick to show the BOUNDS of the screen to ensure your controls fit on the screen
            if (e.OriginalKey == VirtualKey.GamepadX || e.OriginalKey == VirtualKey.F1)
            {
                ToggleMonitorBoundsLines();
                e.Handled = true;
            }

        }

        private void ToggleMonitorBoundsLines()
        {
            if (visibleBounds == null)
            {
                visibleBounds = new Border()
                {
                    BorderBrush = new SolidColorBrush(Color.FromArgb(100, 235, 0, 0)),
                    IsHitTestVisible = false,
                    BorderThickness = new Thickness(48, 27, 48, 27),
                    Visibility = Visibility.Collapsed
                };
                childOfRoot.Children.Add(visibleBounds);
            }

            visibleBounds.Visibility = visibleBounds.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        }

        public void OpenNavePane()
        {
            TogglePaneButton.IsChecked = true; // calls TogglePaneButton_Checked
            NavPaneDivider.Visibility = Visibility.Visible;
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = this.BackRequested();
            }
        }

        // returns handled
        private bool BackRequested()
        {
            // Get a hold of the current frame so that we can inspect the app back stack.
            if (this.AppFrame == null)
                return false;

            // Check to see if this is the top-most page on the app back stack.
            if (this.AppFrame.CanGoBack)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                this.AppFrame.GoBack();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Navigate to the Page for the selected <paramref name="listViewItem"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listViewItem"></param>
        private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            if (item != null)
            {
                if (item.DestPage != null)
                //&& item.DestPage != this.AppFrame.CurrentSourcePageType                
                {
                    this.AppFrame.Navigate(item.DestPage, item.Arguments);
                }
            }
        }

        /// <summary>
        /// Ensures the nav menu reflects reality when navigation is triggered outside of
        /// the nav menu buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                var item = (from p in this.navlist where p.DestPage == e.SourcePageType select p).FirstOrDefault();
                if (item == null && this.AppFrame.BackStackDepth > 0)
                {
                    // In cases where a page drills into sub-pages then we'll highlight the most recent
                    // navigation menu item that appears in the BackStack
                    foreach (var entry in this.AppFrame.BackStack.Reverse())
                    {
                        item = (from p in this.navlist where p.DestPage == entry.SourcePageType select p).FirstOrDefault();
                        if (item != null)
                            break;
                    }
                }

                var container = (ListViewItem)NavMenuList.ContainerFromItem(item);

                // While updating the selection state of the item prevent it from taking keyboard focus.  If a
                // user is invoking the back button via the keyboard causing the selected nav menu item to change
                // then focus will remain on the back button.
                if (container != null) container.IsTabStop = false;
                NavMenuList.SetSelectedItem(container);
                if (container != null) container.IsTabStop = true;
            }
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
        }

        /// <summary>
        /// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        /// using the associated Label of each item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        private void TenFootModeToggle_Click(object sender, RoutedEventArgs e)
        {
            App.IsTenFoot = !App.IsTenFoot;
            UpdateTenFootMode();
        }

        private void UpdateTenFootMode()
        {

            if (App.IsTenFootPC)
            {
                // Not having any mouse might be an option for your application
                //CoreWindow.GetForCurrentThread().PointerCursor = null;

                // TODO: 1.1 - If the app is running in Ten Foot mode, lets make sure we are full screen on a desktop / tablet
                // ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

                if (!(LayoutRoot.Child is Viewbox))
                {
                    Viewbox viewbox = new Viewbox();
                    LayoutRoot.Child = viewbox;
                    viewbox.Child = childOfRoot;
                    childOfRoot.Height = 540;
                    childOfRoot.Width = 960;
                }

                // TODO: 5.0 - We can set the theme here to Dark when in Ten Foot Mode
                // this.RequestedTheme = ElementTheme.Dark;

                // TODO: 5.2 - Make the xbox sounds even when on PC because in Ten Foot mode we want that experience.
                //ElementSoundPlayer.State = ElementSoundPlayerState.On;
            }
            else
            {
                // We are not an XBOX, or have not set TenFootMode override
                CoreWindow.GetForCurrentThread().PointerCursor = _defaultCursor;

                // Full screen is not the normal mode on a non XBOX machine
                ApplicationView.GetForCurrentView().ExitFullScreenMode();

                if (LayoutRoot.Child is Viewbox)
                {
                    (LayoutRoot.Child as Viewbox).Child = null;
                    LayoutRoot.Child = childOfRoot;
                    childOfRoot.Height = double.NaN;
                    childOfRoot.Width = double.NaN;
                }

                // Use the default Theme on the PC
                this.RequestedTheme = ElementTheme.Default;

                // Use the sound defaults on the PC
                ElementSoundPlayer.State = ElementSoundPlayerState.Auto;
            }
        }
    }
}
