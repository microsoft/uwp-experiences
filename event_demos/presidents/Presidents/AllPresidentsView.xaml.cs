using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Presidents
{
    public sealed partial class AllPresidentsView : Page
    {
        public AllPresidentsView()
        {
            this.InitializeComponent();
            gridView.ItemsSource = President.FilteredPresidents;
            gridView.ItemClick += Gv_ItemClick;
            gridView.KeyDown += Gv_KeyDown;
            scrollViewer.KeyDown += ScrollViewer_KeyDown;

            // potential demo
            gridView.IsItemClickEnabled = true;

            gridView.GotFocus += GridView_GotFocus;
        }

        // keep focusrect away from the edge of the screen
        private void GridView_GotFocus(object sender, RoutedEventArgs e)
        {
            if (App.IsTenFoot)
            {
                var padding = 47; // pick your favorite number
                var item = (FocusManager.GetFocusedElement() as Control);
                if (item != null)
                {
                    var itemTop = item.TransformToVisual(scrollViewer).TransformPoint(new Windows.Foundation.Point(0, 0)).Y;
                    var itemBottom = item.TransformToVisual(scrollViewer).TransformPoint(new Windows.Foundation.Point(item.ActualWidth, item.ActualHeight)).Y;

                    if (itemTop < padding)
                        scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + itemTop - padding, null);
                    else if (itemBottom > scrollViewer.ViewportHeight - padding)
                        scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + itemBottom - scrollViewer.ViewportHeight + padding, null);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null && e.Parameter != "")
                gridView.ItemsSource = e.Parameter;
        }

        private void ScrollViewer_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.GamepadDPadUp
                || e.OriginalKey == Windows.System.VirtualKey.GamepadLeftThumbstickUp)
            {
                // don't move focus into nav bar
                e.Handled = true;
            }
            else if (e.OriginalKey == Windows.System.VirtualKey.GamepadDPadDown
                || e.OriginalKey == Windows.System.VirtualKey.GamepadLeftThumbstickDown)
            {
                // don't move focus into nav bar
                e.Handled = true;
            }
        }

        private void Gv_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.GamepadDPadUp
                || e.OriginalKey == Windows.System.VirtualKey.GamepadLeftThumbstickUp)
            {
                // scroll banner image into view
                scrollViewer.ChangeView(0, 0, null, false);
                e.Handled = true;
            }
        }

        private void Gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(DetailsView), e.ClickedItem);           
        }
    }
}
