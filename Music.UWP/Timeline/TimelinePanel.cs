using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Controls
{
    public class TimelinePanel : Panel
    {

        private const double _scrollRate = 100;
        private int _originalIndex;
        private bool _manipulating = false;

        public TimelinePanel()
        {
            //ItemIndex = 0;
            Background = new SolidColorBrush(Colors.Transparent);
            ManipulationStarted += TimelinePanel_ManipulationStarted;
            ManipulationDelta += TimelinePanel_ManipulationDelta;
            ManipulationCompleted += TimelinePanel_ManipulationCompleted;
            Tapped += TimelinePanel_Tapped;
            PointerWheelChanged += TimelinePanel_PointerWheelChanged;

            ManipulationMode = Orientation == Orientation.Vertical ?
                Windows.UI.Xaml.Input.ManipulationModes.TranslateY : Windows.UI.Xaml.Input.ManipulationModes.TranslateX;
        }

        private void TimelinePanel_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ItemIndex += e.GetCurrentPoint(null).Properties.MouseWheelDelta > 0 ? -1 : 1;
            e.Handled = true;
        }

        private void TimelinePanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            List<UIElement> elements = new List<UIElement>(
                VisualTreeHelper.FindElementsInHostCoordinates(
                    e.GetPosition(Window.Current.Content), this));

            TimelineItem item = elements.Where(el => el is TimelineItem).FirstOrDefault() as TimelineItem;

            if (item != null)
            {
                ItemIndex = Children.IndexOf(item);
            }
        }

        private void TimelinePanel_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            var value = Orientation == Orientation.Horizontal ? e.Cumulative.Translation.X : e.Cumulative.Translation.Y;
            var scalledValue = (int)(value / _scrollRate) * -1;
            ItemIndex = _originalIndex + scalledValue;
        }

        private void TimelinePanel_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            _manipulating = true;
            _originalIndex = ItemIndex;
        }

        private void TimelinePanel_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            _manipulating = false;
            InvalidateArrange();
        }

        private DataTemplate _itemTemplate;

        public DataTemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set { _itemTemplate = value; }
        }

        public int ItemIndex
        {
            get { return (int)GetValue(ItemIndexProperty); }
            set
            {
                if (value < 0 || value >= Children.Count)
                    return;

                SetValue(ItemIndexProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TopItemIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemIndexProperty =
            DependencyProperty.Register("ItemIndex", typeof(int), typeof(TimelinePanel), new PropertyMetadata(0, OnPropertyChanged));


        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TimelinePanel), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelinePanel).InvalidateArrange();
            var timeline = (d as TimelinePanel);
            
            timeline.ManipulationMode = (Orientation)e.NewValue == Orientation.Vertical ? 
                Windows.UI.Xaml.Input.ManipulationModes.TranslateY : Windows.UI.Xaml.Input.ManipulationModes.TranslateX;
        }

        public double ItemSpacing
        {
            get { return (double)GetValue(ItemSpacingProperty); }
            set { SetValue(ItemSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSpacingProperty =
            DependencyProperty.Register("ItemSpacing", typeof(double), typeof(TimelinePanel), new PropertyMetadata(32d, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
                if (i < ItemIndex)
                {
                    //item is above
                    for (int j = i; j < this.ItemIndex; ++j)
                    {
                        var value = Orientation == Orientation.Vertical ? Children[j].DesiredSize.Height : Children[j].DesiredSize.Width;
                        deltaFromSelectedItem -= (value + ItemSpacing);
                    }
                }
                else if (i > ItemIndex)
                {
                    for (int j = this.ItemIndex; j < i; ++j)
                    {
                        var value = Orientation == Orientation.Vertical ? Children[j].DesiredSize.Height : Children[j].DesiredSize.Width;
                        deltaFromSelectedItem += (value + ItemSpacing);
                    }
                }

                var childLeft = centerLeft - (child.DesiredSize.Width / 2) + 
                    (Orientation == Orientation.Vertical ? 0 : deltaFromSelectedItem);
                var childTop = centerTop - (child.DesiredSize.Height / 2) +
                    (Orientation == Orientation.Vertical ? deltaFromSelectedItem : 0);

                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));

                var visual = ElementCompositionPreview.GetElementVisual(child);
                var offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation();
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(200);
                offsetAnimation.InsertKeyFrame(1, new System.Numerics.Vector3((float)childLeft, (float)childTop, 0));
                visual.StartAnimation("Offset", offsetAnimation);

                if (!_manipulating)
                {
                    (child as TimelineItem).IsInFocus = i == ItemIndex;
                }
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

            return Children.ElementAt(ItemIndex) as TimelineItem;
        }
    }
}
