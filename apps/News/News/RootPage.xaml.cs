using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace News
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootPage : Page
    {
        public static RootPage Current;
        private int _imageCount;

        public int ImageCount
        {
            get { return _imageCount; }
            set
            {
                _imageCount = value;
                UpdateSize();
            }
        }

        int _imageIndex = 0;

        public RootPage()
        {
            Current = this;
            Window.Current.SizeChanged += Current_SizeChanged;
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainFrame.Navigated += RootFrame_Navigated;
            MainFrame.Navigate(typeof(MainPage));
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            Gradient.Height = Window.Current.Bounds.Height / 2 * (ImageCount + 2);
            Gradient.Width = Window.Current.Bounds.Width;
        }

        public async Task UpdateBackground(string uri, int gradientIndex)
        {
            var source = await loadImageAsync(uri);
            var image = BackgroundContainer.Children.ElementAt(_imageIndex) as Image;
            image.Fade(duration: 0, value: 0).Start();
            image.Source = source;

            _imageIndex = (_imageIndex + 1) % 2;

            var otherImage = BackgroundContainer.Children.ElementAt(_imageIndex) as Image;
            otherImage.Fade(duration: 700, value: 0).Start();
            image.Fade(duration: 700, value: 1).Start();
        }

        private async Task<BitmapImage> loadImageAsync(string path)
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri(path)).OpenReadAsync());

            return bitmapImage;
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = MainFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;

        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }
    }
}
