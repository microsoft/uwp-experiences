using Microsoft.Toolkit.Uwp.UI.Animations;
using News.Data;
using News.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace News
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        List<NewsItem> Items;
        NewsItem Item;
        
        private bool section3FirstTime = true;
        private bool section4FirstTime = true;
        private bool relatedsectionFirstTime = true;
        DispatcherTimer videoControlsTimer;

        bool IsFullscreen = false;

        List<AnimatableSection> animatableSections = new List<AnimatableSection>();

        public DetailsPage()
        {
            Items = NewsItem.GetData().Take(6).ToList();
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;
            CoreWindow.GetForCurrentThread().KeyDown += DetailsPage_KeyDown;

            Loaded += DetailsPage_Loaded;
            
            UpdateSize(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));

            RootElement.ViewChanged += RootElement_ViewChanged;

            animatableSections.Add(new AnimatableSection(Section1, Section1Animate));
            animatableSections.Add(new AnimatableSection(Section2, Section2Animate));
            animatableSections.Add(new AnimatableSection(Section3, Section3Animate));
            animatableSections.Add(new AnimatableSection(Section4, Section4Animate));
            animatableSections.Add(new AnimatableSection(VideoSection, VideoSectionAnimate));
            animatableSections.Add(new AnimatableSection(RelatedSection, RelatedSectionAnimate));
        }

        private void DetailsPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.GamepadX:
                    RelatedGridView.Focus(FocusState.Keyboard);
                    break;
            }
        }

        private void DetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateSections();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var item = e.Parameter as NewsItem;
            if (item != null)
            {
                Item = item;
            }

            var animationService = ConnectedAnimationService.GetForCurrentView();

            var titleAnimation = animationService.GetAnimation("Title");
            if (titleAnimation != null)
            {
                titleAnimation.TryStart(TitleLine);
            }

            var summaryAnimation = animationService.GetAnimation("Summary");
            if (summaryAnimation != null)
            {
                summaryAnimation.TryStart(SummaryText);
            }

            var likesAnimation = animationService.GetAnimation("Likes");
            if (likesAnimation != null)
            {
                likesAnimation.TryStart(LikesStack);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (VideoPlayer.IsFullWindow)
            {
                e.Cancel = true;
                VideoPlayer.IsFullWindow = false;
                VideoPlayer.AreTransportControlsEnabled = false;
                PlayButton.Focus(FocusState.Keyboard);
                return;
            }

            if (VideoPlayer.Source != null)
            {
                VideoPlayer.Source = null;
                VideoPlayer.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            }

            base.OnNavigatingFrom(e);
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpdateSize(e.Size);
            AnimateSections();
        }

        private void UpdateSize(Size size)
        {
            HeroGrid.Height = size.Height;
            Section1.Height = size.Height - 100;
        }

        private void RootElement_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            AnimateSections();
        }

        private void AnimateSections()
        {
            foreach (var section in animatableSections)
            {
                if (IsVisibileToUser(section.Element, RootElement))
                {
                    section.Animate();
                }
            }
        }

        private bool IsVisibileToUser(FrameworkElement element, FrameworkElement container)
        {
            if (element == null || container == null)
                return false;

            if (element.Visibility != Visibility.Visible)
                return false;

            Rect elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return (elementBounds.Height - (containerBounds.Height - elementBounds.Top)) < (elementBounds.Height / 2);
        }

        private void Section1Animate()
        {
            AuthorLine.Offset(offsetY: 20, duration: 0).Then().Fade(1).Offset().Start();
            GlifMain.Offset(offsetY: 20, duration: 0).Then().Fade(1).Offset().Start();
        }

        private void Section2Animate()
        {
            Section2Image.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().Start();
            Section2Border.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().SetDelay(100).Start();
            Section2Text.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().SetDelay(200).Start();
        }

        private void Section3Animate()
        {
            Section3Image.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().Start();
            Section3Text1.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().SetDelay(100).Start();
            var anim = Section3Text2.Offset(offsetY: 20, duration: 0).Then().Fade(1).Offset();

            anim.SetDelay(section3FirstTime ? 1000 : 200);
            section3FirstTime = false;

            anim.Start();
        }

        private bool section4animating = false;

        private async void Section4Animate()
        {
            if (section4animating) return;
            section4animating = true;
            if (section4FirstTime)
            {
                section4FirstTime = false;
                Section4Arrow.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, Section4Arrow.DesiredSize.Width, Section4Arrow.DesiredSize.Height) };
                Section4Arrow.Clip.Transform = new CompositeTransform() { ScaleX = 1, ScaleY = 0 };
                Section4Arrow.Opacity = 1;

                await Section4Arrow.Reveal(duration: 1500).StartAsync();
            }

            Section4Glif.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().Start();
            Section4Text.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().SetDelay(100).Start();
            section4animating = false;
        }

        private void VideoSectionAnimate()
        {
            VideoButton.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().Start();
            VideoDescription.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().Start();
        }

        private void RelatedSectionAnimate()
        {
            if (relatedsectionFirstTime)
            {
                relatedsectionFirstTime = false;
                RelatedText.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, RelatedText.DesiredSize.Width, RelatedText.DesiredSize.Height) };
                RelatedText.Clip.Transform = new CompositeTransform() { ScaleX = 0, ScaleY = 1 };
                RelatedText.Opacity = 1;

                RelatedText.Reveal().StartAsync();
                RelatedGridView.Offset(offsetY: 40, duration: 0).Then().Fade(1).Offset().SetDelay(100).Start();
            }
        }

        private void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Source = MediaSource.CreateFromUri(new Uri("https://adxlive.blob.core.windows.net/asset-3bc60f5a-b30a-4e7d-9ba1-f2a9ce0fbe13/mjsNapol01_high_high.mp4?sv=2012-02-12&sr=c&si=c1742964-8afb-4eb8-8aea-044beb817f78&sig=EeHuyIoVVavEy84V44K2Dkh%2FMNq6yuwV%2BnaXhrwS0Hw%3D&st=2016-08-28T00%3A18%3A44Z&se=2116-08-28T00%3A18%3A44Z"));

            VideoPlayer.Visibility = Visibility.Visible;
            VideoPlayerShadow.Visibility = Visibility.Visible;

            VideoPlayer.AutoPlay = true;
            VideoPlayer.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            VideoPlayer.TransportControls.IsFullWindowButtonVisible = false;
            VideoPlayer.TransportControls.IsFastForwardButtonVisible = true;
            VideoPlayer.TransportControls.IsFastForwardEnabled = true;
            VideoPlayer.TransportControls.IsFastRewindButtonVisible = true;
            VideoPlayer.TransportControls.IsFastRewindEnabled = true;

            VideoTransportControls.Visibility = Visibility.Visible;

            (sender as Button).Fade(0).Start();
            VideoPlayer.Fade(1).Start();
            VideoTransportControls.Fade(1).SetDelay(200).Start();

            Section4.XYFocusDown = PlayButton;
            RelatedGridView.XYFocusUp = PlayButton;

            (sender as Button).IsEnabled = false;
        }

        private void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            var t = PlayButton.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackSession.PlaybackState)
                {
                    case Windows.Media.Playback.MediaPlaybackState.Playing:
                        PlayPauseIcon.Symbol = Symbol.Pause;
                        PlayButton.IsEnabled = true;
                        break;
                    case Windows.Media.Playback.MediaPlaybackState.Paused:
                        PlayPauseIcon.Symbol = Symbol.Play;
                        PlayButton.IsEnabled = true;
                        break;
                    default:
                        PlayButton.IsEnabled = true;
                        break;
                }
            });
            
        }

        private void VideoTransportControls_GotFocus(object sender, RoutedEventArgs e)
        {
            VideoTransportControls.Fade(1).Start();

            if (videoControlsTimer == null)
            {
                videoControlsTimer = new DispatcherTimer();
                videoControlsTimer.Tick += VideoControlsTimer_Tick;
                videoControlsTimer.Interval = TimeSpan.FromSeconds(2);
            }

            videoControlsTimer.Start();
        }

        private void VideoControlsTimer_Tick(object sender, object e)
        {
            VideoTransportControls.Fade(0).Start();
        }

        private void VideoTransportControls_LostFocus(object sender, RoutedEventArgs e)
        {
            VideoTransportControls.Fade(0).Start();
            if (videoControlsTimer != null)
            {
                videoControlsTimer.Stop();
            }
        }

        private void PlayButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (VideoPlayer.MediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                VideoPlayer.MediaPlayer.Pause();
            }
            else if (VideoPlayer.MediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                VideoPlayer.MediaPlayer.Play();
            }
        }

        private void FullScreenButton_Clicked(object sender, RoutedEventArgs e)
        {
            VideoPlayer.AreTransportControlsEnabled = true;
            VideoPlayer.IsFullWindow = true;
        }

        private void VideoTransportControls_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VideoTransportControls.Fade(1).Start();
        }

        private void VideoTransportControls_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VideoTransportControls.Fade(0).Start();
        }

        private void VideoTransportControls_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }

    internal class AnimatableSection
    {
        public FrameworkElement Element { get; set; }
        private Action Animation { get; set; }

        private bool _alreadyAnimated = false;

        public AnimatableSection(FrameworkElement element, Action animation)
        {
            Element = element;
            Animation = animation;
        }

        public void Animate()
        {
            if (_alreadyAnimated) return;

            _alreadyAnimated = true;
            Animation?.Invoke();
        }
    }
}
