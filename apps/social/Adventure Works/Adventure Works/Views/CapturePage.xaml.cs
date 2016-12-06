using Adventure_Works.Data;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Adventure_Works
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CapturePage : Page
    {
        private Camera _camera;
        private double _transitionDuration = 300;

        public CapturePage()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            this.InitializeComponent();

            this.SizeChanged += CapturePage_SizeChanged;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetSize();

            _camera = new Camera(PreviewControl);
            _camera.PictureCaptured += _camera_PictureCaptured;
            _camera.SmileDetectionChanged += _camera_SmileDetectionChanged;

            await _camera.Initialize();

            if (await _camera.CanToggleCameraAsync())
            {
                CameraControlsChangeCameraButton.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            _camera.StopAsync();
        }

        private void CapturePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {
            var minSize = Math.Min(Window.Current.Bounds.Height, Window.Current.Bounds.Width);
            PreviewControl.Height = PreviewControl.Width = minSize - 40;
        }

        private void _camera_SmileDetectionChanged(object sender, bool enabled)
        {
            if (enabled)
            {
                CameraControlsCaptureButton.Offset(0, 100, _transitionDuration).Start();
                CameraControlsChangeCameraButton.Offset(200, 0, _transitionDuration).Start();
                CameraButtons.Scale(2, 2, 50, 0, _transitionDuration).Start();
                CameraControlsSmileText.Fade(1, _transitionDuration).Start();
            }
            else
            {
                CameraControlsCaptureButton.Offset(duration: _transitionDuration).Start();
                CameraControlsChangeCameraButton.Offset(duration: _transitionDuration).Start();
                CameraButtons.Scale(1, 1, 50, 0, _transitionDuration).Start();
                CameraControlsSmileText.Fade(0, _transitionDuration).Start();
            }
        }

        private async void _camera_PictureCaptured(object sender, PictureCapturedEventArgs e)
        {

            var photo = new PhotoData()
            {
                DateTime = DateTime.Now,
                Uri = e.File.Path,
            };

            await _camera.StopAsync();
            await PhotoPreviewView.ShowAndWaitAsync(photo);
            await _camera.Initialize();

            var t = Task.Run(async () =>
            {
                await DataProvider.Instance.SavePhotoAsync(photo);
            });

            CameraControlsCaptureButton.IsEnabled = true;
            if (App.IsXbox())
            {
                CameraControlsCaptureButton.Focus(FocusState.Keyboard);
            }
        }

        private async void ChangeCamera_Click(object sender, RoutedEventArgs e)
        {
            await _camera.ToggleCameraDeviceAsync();
        }

        private void CaptureWhenSmiling_Click(object sender, RoutedEventArgs e)
        {
            _camera.IsSmileDetectionEnabled = !_camera.IsSmileDetectionEnabled;
        }

        private void CapturePhoto_Click(object sender, RoutedEventArgs e)
        {
            CameraControlsCaptureButton.IsEnabled = true;
            _camera.CapturePhotoAsync();
        }

    }
}
