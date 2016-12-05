using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace News.Controls
{
    public class TimelinePanel : Panel
    {
        public TimelinePanel()
        {
            Tapped += Timeline_Tapped;
            TopItemIndex = 0;
        }

        private void Timeline_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // select item if not selected
            // navigate to item if selected
        }

        private DataTemplate _itemTemplate;

        public DataTemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set { _itemTemplate = value; }
        }

        public int TopItemIndex
        {
            get { return (int)GetValue(TopItemIndexProperty); }
            set
            {
                if (value < 0 || value >= Children.Count)
                    return;

                SetValue(TopItemIndexProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TopItemIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopItemIndexProperty =
            DependencyProperty.Register("TopItemIndex", typeof(int), typeof(TimelinePanel), new PropertyMetadata(0, OnTimelinePropertyChanged));

        private static void OnTimelinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelinePanel).InvalidateArrange();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {

            Double centerLeft = finalSize.Width / 2;
            Double centerTop = finalSize.Height / 2;

            this.Clip = new RectangleGeometry { Rect = new Rect(0, 0, finalSize.Width, finalSize.Height) };

            for (int i = 0; i < this.Children.Count; ++i)
            {
                var child = this.Children[i];
                double deltaFromSelectedItem = 0;
                if (i < TopItemIndex)
                {
                    //item is above
                    for (int j = i; j < this.TopItemIndex; ++j)
                    {
                        deltaFromSelectedItem -= this.Children[j].DesiredSize.Height;
                    }
                }
                else if (i > TopItemIndex)
                {
                    for (int j = this.TopItemIndex; j < i; ++j)
                    {
                        deltaFromSelectedItem += this.Children[j].DesiredSize.Height;
                    }
                }

                var childLeft = centerLeft - (child.DesiredSize.Width / 2);
                var childTop = centerTop - (child.DesiredSize.Height / 2) + deltaFromSelectedItem;

                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));

                var visual = ElementCompositionPreview.GetElementVisual(child);
                var offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation();
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(200);
                offsetAnimation.InsertKeyFrame(1, new System.Numerics.Vector3((float)childLeft, (float)childTop, 0));
                visual.StartAnimation("Offset", offsetAnimation);

                (child as TimelineItem).IsInFocus = i == TopItemIndex;
            }

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.Clip = new RectangleGeometry { Rect = new Rect(0, 0, availableSize.Width, availableSize.Height) };

            foreach (var child in this.Children)
            {
                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            }
            return (availableSize);
        }

        public UIElement AddElementToPanel(UIElement element)
        {
            if (!(element is TimelineItem))
            {
                var item = new TimelineItem();
                item.IsActionable = false;
                item.Content = element;
                element = item;
            }

            Children.Add(element);

            return element;
        }

        public TimelineItem GetTopItem()
        {
            if (Children.Count == 0) return null;

            return Children.ElementAt(TopItemIndex) as TimelineItem;
        }
    }
}
