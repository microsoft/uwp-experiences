using Adventure_Works.Data;
using Adventure_Works.Rome;
using Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Adventure_Works
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SlideshowPage : Page
    {
        Adventure _adventure;
        DispatcherTimer _timer;

        bool _navVisible;

        public SlideshowPage()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
            this.SizeChanged += (s, e) => UpdateSize();
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;

                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonInactiveForegroundColor = Colors.LightGray;

                    titleBar.BackgroundColor = Colors.Transparent;
                    titleBar.InactiveBackgroundColor = Colors.Transparent;
                }
            }

            ConnectedService.Instance.ReceivedMessageFromClient += Instance_ReceivedMessageFromClient;
            ConnectedService.Instance.StartHosting();

            var id = e.Parameter;
            _adventure = await DataProvider.Instance.GetAdventure(id as string);

            PhotoTimeline.ItemsSource = _adventure.Photos;

            UpdateSize();

            if (App.IsXbox())
            {
                PhotoTimeline.IsHitTestVisible = false;
                NextButton.Focus(FocusState.Keyboard);
            }

            SlideTextBlock.Text = $"Slide {PhotoTimeline.CurrentItemIndex + 1} of {_adventure.Photos.Count}";
            PhotoTimeline.ItemIndexChanged += TimelinePanel_ItemIndexChanged;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            PointerMoved += SlideshowPage_PointerMoved;
            KeyDown += SlideshowPage_KeyDown;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ConnectedService.Instance.StopHosting();
            ConnectedService.Instance.ReceivedMessageFromClient -= Instance_ReceivedMessageFromClient;

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
                    titleBar.InactiveBackgroundColor = brandColorLight;
                }
            }

            base.OnNavigatedFrom(e);
        }

        private void UpdateSize()
        {
            var size = Math.Min(ActualHeight, ActualWidth) - 20;

            foreach (var view in PhotoTimeline.FindChildren<SlideshowSlideView>())
            {
                view.Height = view.Width = size;
            }

            if (PhotoTimeline.TimelinePanel != null) PhotoTimeline.TimelinePanel.ItemSpacing = ActualWidth / 2;
        }

        private void TimelinePanel_ItemIndexChanged(object sender, TimelinePanelItemIndexEventArgs e)
        {
            SlideTextBlock.Text = $"Slide {e.NewValue + 1} of {_adventure.Photos.Count}";
        }

        private void SlideshowPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.GamepadRightShoulder)
            {
                Next();
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.GamepadLeftShoulder)
            {
                Previous();
                e.Handled = true;
            }
            else if (!_navVisible)
            {
                _navVisible = true;
                e.Handled = true;
                if (App.IsXbox())
                    NextButton.Focus(FocusState.Keyboard);

                ControlsContainer.IsHitTestVisible = true;
                ControlsContainer.Fade(1, 200).Start();
            }
            
            _timer.Start();
        }

        private void SlideshowPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_navVisible)
            {
                _navVisible = true;
                ControlsContainer.IsHitTestVisible = true;
                ControlsContainer.Fade(1, 200).Start();
            }

            _timer.Start();
        }

        private void _timer_Tick(object sender, object e)
        {
            _timer.Stop();
            ControlsContainer.Fade(0, 1000).Start();
            ControlsContainer.IsHitTestVisible = false;
            _navVisible = false;
        }

        private void SlideshowSlideView_Loaded(object sender, RoutedEventArgs e)
        {
            var size = Math.Min(ActualHeight, ActualWidth) - 20;
            var view = sender as SlideshowSlideView;
            view.Height = view.Width = size;
        }

        private void Next()
        {
            if (PhotoTimeline.CurrentItemIndex != _adventure.Photos.Count)
            {
                PhotoTimeline.CurrentItemIndex++;
                SendIndexUpdate();
            }
        }

        private void Previous()
        {
            if (PhotoTimeline.CurrentItemIndex > 0)
            {
                PhotoTimeline.CurrentItemIndex--;
                SendIndexUpdate();
            }
        }

        private async Task<bool> SendIndexUpdate()
        {
            var message = new ValueSet();
            message.Add("index", PhotoTimeline.CurrentItemIndex);
            var response = await Rome.ConnectedService.Instance.SendMessageFromHostAsync(message, SlideshowMessageTypeEnum.UpdateIndex);
            var success = response != null && response.ContainsKey("AOK");

            return success;
        }

        private void Instance_ReceivedMessageFromClient(object sender, SlideshowMessageReceivedEventArgs e)
        {
            switch (e.QueryType)
            {
                case SlideshowMessageTypeEnum.Status:
                    e.ResponseMessage.Add("index", PhotoTimeline.CurrentItemIndex);
                    e.ResponseMessage.Add("adventure_id", _adventure.Id.ToString());
                    break;
                case SlideshowMessageTypeEnum.UpdateIndex:
                    if (e.Message.ContainsKey("index"))
                    {
                        var index = (int)e.Message["index"];
                        PhotoTimeline.CurrentItemIndex = index;
                    }
                    break;
                case SlideshowMessageTypeEnum.UpdateStrokes:
                    if (e.Message.ContainsKey("stroke_data"))
                    {
                        var data = (byte[])e.Message["stroke_data"];
                        var index = (int)e.Message["index"];
                        HandleStrokeData(data, index);
                    }
                    break;
                default:
                    break;
            }

            e.ResponseMessage.Add("AOK", "");
        }

        private async Task HandleStrokeData(byte[] data, int index)
        {
            if (index == PhotoTimeline.CurrentItemIndex)
            {
                var view = PhotoTimeline.FindChildren<SlideshowSlideView>().ElementAt(index);
                await view.UpdateStrokes(data);
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

    }
}
