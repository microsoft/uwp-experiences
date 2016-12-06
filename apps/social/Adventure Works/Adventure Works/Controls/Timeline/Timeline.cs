using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Controls
{
    [TemplatePart(Name = PartTimelinePanel, Type = typeof(TimelinePanel))]
    public sealed class Timeline : ContentControl
    {
        public TimelinePanel TimelinePanel { get; private set; }

        private const string PartTimelinePanel = "TPanel";

        public event EventHandler<TimelinePanelItemIndexEventArgs> ItemIndexChanged;

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
                TimelinePanel.ItemIndexChanged += TimelinePanel_ItemIndexChanged;
                TimelinePanel.Orientation = Orientation;
                UpdateItems();
                TimelinePanel.ItemIndex = (int)GetValue(CurrentItemIndexProperty);
            }

            KeyDown += Timeline_KeyDown;
            KeyUp += Timeline_KeyUp;

            base.OnApplyTemplate();
        }

        private void TimelinePanel_ItemIndexChanged(object sender, TimelinePanelItemIndexEventArgs e)
        {
            ItemIndexChanged?.Invoke(this, e);
        }

        public event EventHandler<TimelineItemInvokedEventArgs> ItemInvoked;

        private void Timeline_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (TimelinePanel == null)
                return;
            
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    if (Orientation == Orientation.Vertical)
                    {
                        TimelinePanel.ItemIndex++;
                        e.Handled = true;
                    }
                    break;
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    if (Orientation == Orientation.Vertical)
                    {
                        TimelinePanel.ItemIndex--;
                        e.Handled = true;
                    }
                    break;
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadDPadLeft:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                    if (Orientation == Orientation.Horizontal)
                    {
                        TimelinePanel.ItemIndex--;
                        e.Handled = true;
                    }
                    break;
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadDPadRight:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                    if (Orientation == Orientation.Horizontal)
                    {
                        TimelinePanel.ItemIndex++;
                        e.Handled = true;
                    }
                    break;
                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.GamepadA:
                    var item = TimelinePanel.GetTopItem();
                    if (item == null || !item.IsActionable)
                        break;
                    item.Scale(duration: 200, centerX: 0.5f, centerY: 0.5f, scaleX: 1f, scaleY: 1f).StartAsync();
                    ElementSoundPlayer.Play(ElementSoundKind.Invoke);
                    ItemInvoked?.Invoke(this, new Controls.TimelineItemInvokedEventArgs() { Container = item });
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
                   // e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    //e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.GamepadA:
                    var item = TimelinePanel.GetTopItem();
                    if (item == null || !item.IsActionable)
                        break;
                    item.Scale(duration: 200, centerX: 0.5f, centerY: 0.5f, scaleX: 0.9f, scaleY: 0.9f).StartAsync();
                    break;

            }
        }

        private DataTemplate _itemTemplate;

        public DataTemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set { _itemTemplate = value; }
        }

        public int CurrentItemIndex
        {
            get
            {
                if (TimelinePanel != null)
                {
                    var index = TimelinePanel.ItemIndex;
                    SetValue(CurrentItemIndexProperty, index);
                    return index;
                }
                return (int)GetValue(CurrentItemIndexProperty);
            }
            set
            {
                SetValue(CurrentItemIndexProperty, value);
                if (TimelinePanel != null)
                {
                    TimelinePanel.ItemIndex = value;
                }
            }
        }

        // Using a DependencyProperty as the backing store for CurrentItemIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentItemIndexProperty =
            DependencyProperty.Register("CurrentItemIndex", typeof(int), typeof(Timeline), new PropertyMetadata(0));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Timeline), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = d as Timeline;

            if (timeline.TimelinePanel == null)
            {
                return;
            }

            timeline.TimelinePanel.Orientation = (Orientation)e.NewValue;
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
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(Timeline), new PropertyMetadata(null, OnTimelineItemSourceChanged));

        private static void OnTimelineItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = d as Timeline;
            timeline.HandleNewItemsSource(e.NewValue, e.OldValue);
        }

        private void HandleNewItemsSource(object newValue, object oldValue)
        {
            if (newValue == null)
            {
                return;
            }

            if (newValue == oldValue)
            {
                return;
            }

            if (oldValue != null)
            {
                var oldObservableList = oldValue as INotifyCollectionChanged;
                if (oldObservableList != null)
                {
                    oldObservableList.CollectionChanged -= ObservableList_CollectionChanged;
                }
            }

            var items = newValue as IEnumerable<object>;
            if (items == null)
                return;

            var observableList = newValue as INotifyCollectionChanged;
            if (observableList != null)
            {
                observableList.CollectionChanged += ObservableList_CollectionChanged;
            }

            UpdateItems();


        }

        private void ObservableList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CoreApplication.MainView.CoreWindow != null)
            {
                var t = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UpdateItems();
                });
            }
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
