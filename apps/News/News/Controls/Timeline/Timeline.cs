using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace News.Controls
{
    [TemplatePart(Name = PartTimelinePanel, Type = typeof(TimelinePanel))]
    public sealed class Timeline : ContentControl
    {
        public TimelinePanel TimelinePanel { get; private set; }

        private const string PartTimelinePanel = "TPanel";

        public Timeline()
        {
            this.DefaultStyleKey = typeof(Timeline);
        }

        protected override void OnApplyTemplate()
        {
            if (TimelinePanel != null)
            {
                // clean up
                TimelinePanel.Children.Clear();
            }

            TimelinePanel = this.GetTemplateChild(PartTimelinePanel) as TimelinePanel;

            if (TimelinePanel != null)
            {
                UpdateItems();
            }

            KeyDown += Timeline_KeyDown;
            KeyUp += Timeline_KeyUp;
            GotFocus += Timeline_GotFocus;

            base.OnApplyTemplate();
        }

        public event EventHandler<TimelineItemInvokedEventArgs> ItemInvoked;

        private void Timeline_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Timeline_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (TimelinePanel == null)
                return;
            
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    TimelinePanel.TopItemIndex++;
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    TimelinePanel.TopItemIndex--;
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.GamepadA:
                    var item = TimelinePanel.GetTopItem();
                    if (item == null || !item.IsActionable)
                        break;
                    item.Scale(duration: 200, centerX: (float)item.DesiredSize.Width / 2, centerY: (float)item.DesiredSize.Height / 2, scaleX: 1f, scaleY: 1f).StartAsync();
                    ElementSoundPlayer.Play(ElementSoundKind.Invoke);
                    ItemInvoked?.Invoke(this, new Controls.TimelineItemInvokedEventArgs() { Item = item });
                    break;
            }
        }

        private void Timeline_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.GamepadA:
                    var item = TimelinePanel.GetTopItem();
                    if (item == null || !item.IsActionable)
                        break;
                    item.Scale(duration: 200, centerX: (float)item.DesiredSize.Width / 2, centerY: (float)item.DesiredSize.Height / 2, scaleX: 0.9f, scaleY: 0.9f).StartAsync();
                    break;

            }
        }

        private DataTemplate _itemTemplate;

        public DataTemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set { _itemTemplate = value; }
        }

        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(UIElement), typeof(TimelinePanel), new PropertyMetadata(null, OnTimelineHeaderPropertyChanged));

        private static void OnTimelineHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            if (e.NewValue == e.OldValue)
                return;

            Timeline timeline = d as Timeline;

            if (timeline == null)
                return;

            if (timeline.TimelinePanel == null)
                return;

            timeline.UpdateItems();
        }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(TimelinePanel), new PropertyMetadata(null, OnTimelineItemSourceChanged));

        private static void OnTimelineItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            if (e.NewValue == e.OldValue)
                return;

            Timeline timeline = d as Timeline;

            if (timeline == null)
                return;

            if (timeline.TimelinePanel == null)
                return;

            var items = e.NewValue as IEnumerable<object>;
            if (items == null)
                return;

            var observableList = e.NewValue as INotifyCollectionChanged;
            if (observableList != null)
            {
                // TODO: handle events for added and removed elements
            }

            timeline.UpdateItems();
        }

        private void UpdateItems()
        {
            IEnumerable<object> items = ItemsSource as IEnumerable<object>;
            if (items == null)
                return;

            if (TimelinePanel == null)
                return;

            TimelinePanel.Children.Clear();
            if (HeaderContent != null)
                TimelinePanel.AddElementToPanel(HeaderContent);

            foreach (var item in items)
            {
                TimelinePanel.AddElementToPanel(CreateItem(item));
            }
        }

        private FrameworkElement CreateItem(object item)
        {
            FrameworkElement element = ItemTemplate.LoadContent() as FrameworkElement;
            if (element == null)
                return null;

            element.DataContext = item;
            
            return element;
        }

    }
}
