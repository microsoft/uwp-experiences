using Microsoft.Toolkit.Uwp.UI.Animations;
using News.Helpers.Composition;
using News.Helpers.Composition.ImageLoader;
using News.Controls;
using News.Data;
using News.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace News
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private List<NewsItem> items;

        public List<NewsItem> Items
        {
            get { return items; }
            set { Set(ref items, value); }
        }

        private NewsItem hero;

        public NewsItem Hero
        {
            get { return hero; }
            set { Set(ref hero, value); }
        }

        List<string> Topics;
        bool _firstRun = true;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.NavigationCacheMode = NavigationCacheMode.Required;
            var items = NewsItem.GetData();
            if (App.IsXbox())
            {
                Hero = items.First();
                items.RemoveAt(0);
            }
            
            Items = items;
            Topics = NewsItem.GetListOfTopics();
            this.Loaded += MainPage_Loaded;
            RootPage.Current.ImageCount = Items.Count;

            this.InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsXbox() && _firstRun)
            {
                _firstRun = false;

                if (Timeline.TimelinePanel.Margin.Left != -180)
                    Timeline.TimelinePanel.Margin = new Thickness(-180, 0, 0, 0);

                SetupCompositionImage(HeroStoryImage, Shadow);

                this.SizeChanged += MainPage_SizeChanged;
            }
            else
            {
                RootPage.Current.UpdateBackground(Items.First().HeroImage, 0);
            }

            if (App.IsXbox())
            {
                Timeline.Focus(FocusState.Programmatic);
                UpdateTimelineItemsSize();
            }

            SectionList.SelectedIndex = 0;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTimelineItemsSize();
        }

        private void UpdateTimelineItemsSize()
        {
            var storyItems = this.FindChildren<TimelineStory>();

            bool isSnapped = Window.Current.Bounds.Width < 960;

            foreach (var storyItem in storyItems)
            {
                storyItem.Width = isSnapped ? 600 : 750;
            }

            if (HeaderRoot != null)
            {
                HeaderRoot.MaxWidth = isSnapped ? 300 : 430;
            }
        }

        private void Timeline_GotFocus(object sender, RoutedEventArgs e)
        {
            Timeline.Fade(1).Start();
        }

        private void Timeline_LostFocus(object sender, RoutedEventArgs e)
        {
            Timeline.Fade(0.6f).Start();
        }

        private void SetupCompositionImage(CompositionImage image, News.Helpers.Composition.CompositionShadow shadow)
        {
            

            var imageVisual = image.SpriteVisual;
            var compositor = imageVisual.Compositor;

            var imageLoader = ImageLoaderFactory.CreateImageLoader(compositor);
            var imageMaskSurface = imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Helpers/Composition/CircleMask.png"));

            var mask = compositor.CreateSurfaceBrush();
            mask.Surface = imageMaskSurface.Surface;

            var source = image.SurfaceBrush as CompositionSurfaceBrush;

            var maskBrush = compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source;

            image.Brush = maskBrush;
            shadow.Mask = maskBrush.Mask;
        }

        private void TimelineItem_HeaderGotFocus(object sender, EventArgs e)
        {
            var item = sender as TimelineItem;
            RootPage.Current.UpdateBackground(Hero.HeroImage, Timeline.TimelinePanel.TopItemIndex);

            HeaderDate.Fade(1).Start();
            HeaderTime.Fade(1).Start();
            HeaderSummary.Fade(1).Start();
            HeaderLikes.Fade(1).Start();
            HeaderImageContainer.Scale(1.2f, 1.2f, 34, 34).Offset().Start();
            HeaderTitle.Offset().Fade(1).Scale().Start();
            CornerClock.Fade(0).Start();
            WeatherIcon.Offset(60).Start();
        }

        private void TimelineItem_HeaderLostFocus(object sender, EventArgs e)
        {
            HeaderDate.Fade(0).Start();
            HeaderTime.Fade(0).Start();
            HeaderSummary.Fade(0).Start();
            HeaderLikes.Fade(0).Start();
            HeaderImageContainer.Scale(1f, 1f, 34, 34).Offset(0, 140).Start();
            HeaderTitle.Offset(190, 70).Fade(0.7f).Scale(0.7f ,0.7f).Start();
            CornerClock.Fade(1).Start();
            WeatherIcon.Offset().Start();
        }

        private void TimelineItem_ItemGotFocus(object sender, EventArgs e)
        {
            var item = sender as TimelineItem;
            var stories = item.FindChildren<TimelineStory>();

            if (stories.Count() == 0)
                return;

            var storie = stories.First();
            storie.AnimateFocus();

            RootPage.Current.UpdateBackground(storie.Item.HeroImage, Timeline.TimelinePanel.TopItemIndex);
        }

        private void TimelineItem_ItemLostFocus(object sender, EventArgs e)
        {
            var item = sender as TimelineItem;
            var stories = item.FindChildren<TimelineStory>();

            if (stories.Count() == 0)
                return;

            stories.First().AnimateFocusLost();
        }

        private void Timeline_ItemInvoked(object sender, TimelineItemInvokedEventArgs e)
        {
            //var container = e.Item.Content as TimelineStory;
            //if (container != null)
            //{
            //    container.PrepareForNavigation();
            //}
            //else
            //{
            //    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Title", HeaderTitle);
            //}

            NewsItem item = e.Item.DataContext as NewsItem;

            Frame.Navigate(typeof(DetailsPage), item);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var shadow = (sender as FrameworkElement).FindChildren<News.Helpers.Composition.CompositionShadow>().FirstOrDefault();
            var image = (sender as FrameworkElement).FindChildren<CompositionImage>().FirstOrDefault();

            if (shadow == null || image == null)
                return;

            SetupCompositionImage(image, shadow);
        }

        private void Timeline_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.GamepadDPadLeft ||
                e.OriginalKey == Windows.System.VirtualKey.GamepadLeftThumbstickLeft)
            {
                if (previousFocus != null)
                {
                    previousFocus?.Focus(FocusState.Keyboard);
                }
                else
                {
                    SectionList.Focus(FocusState.Keyboard);
                }
                e.Handled = true;
            }
        }

        ListViewItem previousFocus;

        private void SectionList_LostFocus(object sender, RoutedEventArgs e)
        {
            previousFocus = e.OriginalSource as ListViewItem;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var index = Items.IndexOf(e.ClickedItem as NewsItem);
            var children = (sender as ListView).FindChildren<ListViewItem>();

            var listViewItem = children.ElementAt(index);

            var titleLine = listViewItem.FindDescendantByName("TitleLine");
            var summaryLine = listViewItem.FindDescendantByName("SummaryLine");
            var likesStack = listViewItem.FindDescendantByName("LikesStack");


            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Title", titleLine);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Summary", summaryLine);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Likes", likesStack);

            var story = e.ClickedItem as NewsItem;

            RootPage.Current.UpdateBackground(story.HeroImage, 0);
            Frame.Navigate(typeof(DetailsPage), story);

        }

        private void Template_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTemplateContentVisibility(sender);
        }

        private void Template_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTemplateContentVisibility(sender);
        }

        private void UpdateTemplateContentVisibility(object sender)
        {
            var grid = sender as Grid;
            var commentStack = grid.FindName("CommentStack") as StackPanel;
            var likeStack = grid.FindName("LikeStack") as StackPanel;

            var width = Window.Current.Bounds.Width;

            if (width > 1000)
            {
                likeStack.Visibility = Visibility.Visible;
                commentStack.Visibility = Visibility.Visible;
            }
            else if (width > 700)
            {
                likeStack.Visibility = Visibility.Collapsed;
                commentStack.Visibility = Visibility.Visible;
            }
            else
            {
                likeStack.Visibility = Visibility.Collapsed;
                commentStack.Visibility = Visibility.Collapsed;
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SectionTitle.Text = e.AddedItems.First() as string;
            Splitter.IsPaneOpen = false;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(propertyName);
            }
        }
    }
}
