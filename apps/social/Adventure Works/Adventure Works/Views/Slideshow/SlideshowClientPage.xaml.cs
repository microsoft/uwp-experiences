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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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
    public sealed partial class SlideshowClientPage : Page
    {
        Adventure _adventure;
        SlideshowSlideView _selectedSlide;

        int _hostSlide = 0;

        public SlideshowClientPage()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string id;

            if (e.Parameter is string)
            {
                id = e.Parameter as string;
                _adventure = await DataProvider.Instance.GetAdventure(id as string);
                PhotoTimeline.ItemsSource = _adventure.Photos;
            }
            else if (e.Parameter is ValueSet)
            {
                var message = e.Parameter as ValueSet;
                if (message.ContainsKey("adventure_id"))
                {
                    id = message["adventure_id"] as string;
                    _adventure = await DataProvider.Instance.GetAdventure(id as string);
                    PhotoTimeline.ItemsSource = _adventure.Photos;

                    if (message.ContainsKey("index"))
                    {
                        PhotoTimeline.CurrentItemIndex = (int)message["index"];
                    }
                }
                else
                {
                    Frame.GoBack();
                    return;
                }
            }
            else
            {
                Frame.GoBack();
                return;
            }

            SlideTextBlock.Text = $"Slide {PhotoTimeline.CurrentItemIndex + 1} of {_adventure.Photos.Count}";
            _hostSlide = PhotoTimeline.CurrentItemIndex;

            PhotoTimeline.ItemIndexChanged += TimelinePanel_ItemIndexChanged;

            ConnectedService.Instance.ReceivedMessageFromHost += Instance_ReceivedMessageFromHost;
            ConnectedService.Instance.HostingConnectionStoped += Instance_HostingConnectionStoped;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ConnectedService.Instance.ReceivedMessageFromHost -= Instance_ReceivedMessageFromHost;
            ConnectedService.Instance.HostingConnectionStoped -= Instance_HostingConnectionStoped;
        }

        private void Instance_HostingConnectionStoped(object sender, EventArgs e)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Frame.GoBack());
        }

        SlideshowSlideView viewToUpdate;
        bool updatingStrokes = false;

        private void TimelinePanel_ItemIndexChanged(object sender, TimelinePanelItemIndexEventArgs e)
        {
            SlideTextBlock.Text = $"Slide {e.NewValue + 1} of {_adventure.Photos.Count}";
        }

        private async Task<bool> SendIndexUpdate()
        {
            var message = new ValueSet();
            message.Add("index", PhotoTimeline.CurrentItemIndex);
            var response = await ConnectedService.Instance.SendMessageFromClientAsync(message, SlideshowMessageTypeEnum.UpdateIndex);
            var success = response != null && response.ContainsKey("AOK");
            if (success)
            {
                _hostSlide = PhotoTimeline.CurrentItemIndex;
            }

            return success;
        }

        private void Instance_ReceivedMessageFromHost(object sender, SlideshowMessageReceivedEventArgs e)
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
                        HandleIndexUpdateFromHost((int)e.Message["index"]);
                    }
                    break;
                default:
                    break;
            }

            e.ResponseMessage.Add("AOK", "");
        }
        
        private void SlideshowSlideView_InkChanged(object sender, EventArgs args)
        {
            var view = sender as SlideshowSlideView;
            SendStrokeUpdates(view);
        }

        private async Task SendStrokeUpdates(SlideshowSlideView view)
        {
            if (updatingStrokes)
            {
                viewToUpdate = view;
                return;
            }

            updatingStrokes = true;
            viewToUpdate = null;

            bool success = false;

            var data = await view.GetStrokeData();
            int index = _adventure.Photos.IndexOf(view.Photo);

            var message = new ValueSet();
            message.Add("stroke_data", data);
            message.Add("index", index);
            var response = await ConnectedService.Instance.SendMessageFromClientAsync(message, SlideshowMessageTypeEnum.UpdateStrokes);
            success = response != null && response.ContainsKey("AOK");

            updatingStrokes = false;
            if (viewToUpdate != null)
                SendStrokeUpdates(viewToUpdate);
        }

        private void SlideshowSlideView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var view = sender as SlideshowSlideView;
            var items = PhotoTimeline.FindChildren<TimelineItem>().ToList();

            if (PhotoTimeline.TimelinePanel.IsLocked)
            {
                PhotoTimeline.TimelinePanel.IsLocked = false;
                HandleIndexUpdateFromHost(_hostSlide);

                for (var i = 0; i < items.Count(); i++)
                {
                    items[i].Fade(1).Start();
                    items[i].IsHitTestVisible = true;
                }
            }
            else
            {
                PhotoTimeline.TimelinePanel.IsLocked = true;

                var photoIndex = _adventure.Photos.IndexOf(view.Photo);
                var size = Math.Min(ActualWidth, ActualHeight);
                float scale = (float)size / (float)view.Height;
                float center = (float)view.Height / 2;

                view.Scale(scale, scale, center, center, duration: 200).Start();
                PhotoTimeline.CurrentItemIndex = photoIndex;
                SendIndexUpdate();

                for (var i = 0; i < items.Count(); i++)
                {
                    if (i != photoIndex)
                    {
                        items[i].Fade(0, 200).Start();
                        items[i].IsHitTestVisible = false;
                    }
                }

                if (_selectedSlide != null && _selectedSlide != view)
                {
                    _selectedSlide?.Scale(1, 1, center, center, 100).Start();
                }
                _selectedSlide = view;

            }

        }

        private void HandleIndexUpdateFromHost(int index)
        {
            _hostSlide = index;
            if (!PhotoTimeline.TimelinePanel.IsLocked)
            {
                var items = PhotoTimeline.FindChildren<TimelineItem>().ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    var view = items[i].Content as SlideshowSlideView;
                    float center = (float)view.Height / 2;

                    if (i != index)
                    {
                        view.Scale(1f, 1f, centerX: center, centerY: center, duration: 200).Start();
                    }
                    else
                    {
                        view.Scale(1.2f, 1.2f, centerX: center, centerY: center, duration: 200).Start();
                    }
                }
            }

        }
    }
}
