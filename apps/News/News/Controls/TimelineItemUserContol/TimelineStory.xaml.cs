using Microsoft.Toolkit.Uwp.UI.Animations;
using News.Helpers.Composition;
using News.Helpers.Composition.ImageLoader;
using News.Data;
using News.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace News.Controls
{
    public sealed partial class TimelineStory : UserControl
    {
        private const double _animationDuration = 300;
        private bool _firstRun = true;

        public TimelineStory()
        {
            Opacity = 0;
            this.InitializeComponent();
            Loaded += TimelineStory_Loaded;
            Loading += TimelineStory_Loading;

            DateContainer.Fade(duration: 0).StartAsync();
            TopLine.Fade(duration: 0).StartAsync();
            SummaryContainer.Fade(duration: 0).StartAsync();
            LikesContainer.Fade(duration: 0).StartAsync();
            ImageContainer.Scale(duration: 0,
                                 centerX: (float)ImageContainer.Width / 2,
                                 centerY: (float)ImageContainer.Height / 2,
                                 scaleX: 0.75f,
                                 scaleY: 0.75f).StartAsync();

            TitleLine.Scale(duration: 0,
                            scaleX: 0.6f,
                            scaleY: 0.6f)
                     .Offset(offsetX: -30,
                             offsetY: 35,
                             duration: 0)
                     .Fade(0.7f, duration: 0).StartAsync();

        }

        private void TimelineStory_Loading(FrameworkElement sender, object args)
        {
           // Opacity = 0;
        }

        private void TimelineStory_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }
        
        public NewsItem Item
        {
            get { return (NewsItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(NewsItem), typeof(TimelineStory), new PropertyMetadata(null));

        private void Setup()
        {
            if (!_firstRun) return;
            _firstRun = false;

            var shadowContainer = ElementCompositionPreview.GetElementVisual(ImageContainer);
            var compositor = shadowContainer.Compositor;

            var image = ImageContainer.FindChildren<CompositionImage>().First();
            var imageVisual = image.SpriteVisual;

            var imageLoader = ImageLoaderFactory.CreateImageLoader(compositor);
            var imageMaskSurface = imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Helpers/Composition/CircleMask.png"));

            var mask = compositor.CreateSurfaceBrush();
            mask.Surface = imageMaskSurface.Surface;

            var source = image.SurfaceBrush as CompositionSurfaceBrush;

            var maskBrush = compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source;

            image.Brush = maskBrush;
            Shadow.Mask = maskBrush.Mask;

            this.Fade(value: 1, delay: 1000).StartAsync();
        }

        public void AnimateFocus()
        {
            DateContainer.Fade(duration: _animationDuration + 300, value: 1, delay: 50).StartAsync();
            TopLine.Fade(duration: _animationDuration + 300, value: 1, delay: 50).StartAsync();
            SummaryContainer.Fade(duration: _animationDuration + 300, value: 1, delay: 50).StartAsync();
            LikesContainer.Fade(duration: _animationDuration + 300, value: 1, delay: 50).StartAsync();
            ImageContainer.Scale(duration: _animationDuration,
                                 centerX: (float)ImageContainer.Width / 2,
                                 centerY: (float)ImageContainer.Height / 2,
                                 scaleX: 1.2f,
                                 scaleY: 1.2f).StartAsync();

            TitleLine.Scale(duration: _animationDuration, scaleX: 1f, scaleY: 1f)
                     .Offset(duration: _animationDuration)
                     .Fade(1f, duration: _animationDuration)
                     .StartAsync();
            this.Fade(1, duration: _animationDuration).Start();
        }

        public void AnimateFocusLost()
        {
            DateContainer.Fade(duration: _animationDuration).StartAsync();
            TopLine.Fade(duration: _animationDuration).StartAsync();
            SummaryContainer.Fade(duration: _animationDuration).StartAsync();
            LikesContainer.Fade(duration: _animationDuration).StartAsync();
            ImageContainer.Scale(duration: _animationDuration,
                                 centerX: (float)ImageContainer.Width / 2,
                                 centerY: (float)ImageContainer.Height / 2,
                                 scaleX: 0.6f,
                                 scaleY: 0.6f).StartAsync();

            TitleLine.Scale(duration: _animationDuration, scaleX: 0.6f, scaleY: 0.6f)
                     .Offset(offsetX: -30,
                             offsetY: 35,
                             duration: _animationDuration)
                     .Fade(0.7f, duration: _animationDuration)
                     .StartAsync();
            this.Fade(0.7f, duration: _animationDuration).Start();
        }

        public void PrepareForNavigation()
        {
            var animationService = ConnectedAnimationService.GetForCurrentView();
            animationService.PrepareToAnimate("Title", TitleLine);
            animationService.PrepareToAnimate("Summary", SummaryContainer);
        }
    }
}
