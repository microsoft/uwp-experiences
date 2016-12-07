using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Presidents
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailsView : Page
    {
        public DetailsView()
        {
            this.InitializeComponent();
            this.inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
            var attr = new Windows.UI.Input.Inking.InkDrawingAttributes();
            attr.Size = new Size(5, 5);
            this.inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attr);

            this.Loaded += DetailsView_Loaded;
            this.KeyDown += DetailsView_KeyDown;
        }

        private void DetailsView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.GamepadLeftShoulder)
            {
                GotoAnother(-1);
                e.Handled = true;
            } else if (e.OriginalKey == Windows.System.VirtualKey.GamepadRightShoulder)
            {
                GotoAnother(1);
                e.Handled = true;
            }
        }

        private void DetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsTenFoot)
                mainBorder.Padding = new Thickness(0,27,48,27);

            inkCanvasHolder.XYFocusLeft = nextButton;
            if (PresIndex() == 0)
                previousButton.IsEnabled = false;
            if (PresIndex() >= President.FilteredPresidents.Count)
            {
                nextButton.IsEnabled = false;
                inkCanvasHolder.XYFocusLeft = previousButton;
                previousButton.Focus(FocusState.Programmatic);
            }
            else
            {
                nextButton.Focus(FocusState.Programmatic);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            GotoAnother(1);
        }

        private int PresIndex()
        {
            return President.FilteredPresidents.IndexOf(this.DataContext as President);
        }

        private void GotoAnother(int increment)
        {
            int index = PresIndex();
            index += increment;
            if (index < 0) index = 0;
            if (index >= President.FilteredPresidents.Count) index = President.FilteredPresidents.Count;
            this.Frame.Navigate(typeof(DetailsView), President.FilteredPresidents[index]);
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            GotoAnother(-1);
        }
    }
}
