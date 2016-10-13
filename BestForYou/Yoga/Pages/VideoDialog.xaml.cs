using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Yoga.Pages
{
    public sealed partial class VideoDialog : ContentDialog
    {
        public VideoDialog()
        {
            this.InitializeComponent();
            this.Loaded += VideoDialog_Loaded;
            this.PointerPressed += (s, e) => this.Hide();
            this.Closing += VideoDialog_Closing;
        }

        private void VideoDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            mediaPlayerElement.MediaPlayer.Pause();
            this.RootGrid.Children.Clear();
        }

        private MediaPlayerElement mediaPlayerElement = new MediaPlayerElement();

        private void VideoDialog_Loaded(object sender, RoutedEventArgs e)
        {
            mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri("http://video.ch9.ms/ch9/2f52/44b36f3c-0822-40b1-9926-6771225a2f52/mjsNapol01.mp4"));
            mediaPlayerElement.AreTransportControlsEnabled = true;
            mediaPlayerElement.TransportControls.IsFullWindowButtonVisible = false;
            mediaPlayerElement.AutoPlay = true;

            this.RootGrid.Children.Add(mediaPlayerElement);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
