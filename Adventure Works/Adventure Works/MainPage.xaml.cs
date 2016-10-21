using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Storage.Streams;
using Microsoft.ProjectOxford.Vision;
using Adventure_Works.CognitiveServices;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Core;

namespace Adventure_Works
{
    public sealed partial class MainPage : Page
    {
        private Camera _camera;

        private double _transitionDuration = 300;

        public MainPage()
        {
            this.InitializeComponent();
            this.GotFocus += MainPage_GotFocus;
        }

        private void MainPage_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(FocusManager.GetFocusedElement().ToString());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            PreviewControl.Blur(20, 0).Scale(1.2f, 1.2f, (float)Window.Current.Bounds.Width / 2, (float)Window.Current.Bounds.Height / 2, 0).Start();

            CameraButtons.Offset(100, 0, 0).Start();
            CameraControlsChangeCameraButton.Offset(200, 0, 0).Start();
            CameraControlsCaptureWhenSmilingButton.Offset(100, 0, 0).Start();

            CameraControlsCaptureButton.Offset(0, 100, 0).Start();

            _camera = new Camera(PreviewControl);
            _camera.PictureCaptured += _camera_PictureCaptured;
            _camera.SmileDetectionChanged += _camera_SmileDetectionChanged;

            if (App.IsXbox())
            {
                MainControlsCaptureButton.Focus(FocusState.Keyboard);
            }

            await _camera.Initialize();

            if (await _camera.CanToggleCameraAsync())
            {
                CameraControlsChangeCameraButton.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _camera.StopAsync();

            base.OnNavigatingFrom(e);
        }

        private void ShowCamera()
        {
            Shade.IsHitTestVisible = false;

            Shade.Fade(0, _transitionDuration).Start();
            MainControls.Offset(0, (float)MainControls.Height, duration: _transitionDuration).Start();
            PreviewControl.Blur(0).Scale(1f, 1f, (float)Window.Current.Bounds.Width / 2, (float)Window.Current.Bounds.Height / 2).Start();

            CameraControlsCaptureButton.Offset(delay: 200, duration: _transitionDuration).Start();

            CameraButtons.Offset(0, 0, _transitionDuration, 200).Start();
            CameraControlsChangeCameraButton.Offset(0, 0, _transitionDuration, 300).Start();
            CameraControlsCaptureWhenSmilingButton.Offset(0, 0, _transitionDuration, 300).Start();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            ((App)(App.Current)).BackRequested += PhotoPreviewView_BackRequested;

            MainControls.IsHitTestVisible = false;

            MainControlsCaptureButton.IsTabStop = false;
            MainControlsBrowseButton.IsTabStop = false;
            MainControlsUploadButton.IsTabStop = false;

            CameraControlsCaptureButton.IsTabStop = true;
            CameraControlsChangeCameraButton.IsTabStop = true;
            CameraControlsCaptureWhenSmilingButton.IsTabStop = true;

            if (App.IsXbox())
            {
                CameraControlsCaptureButton.Focus(FocusState.Keyboard);
            }
        }

        private void HideCamera()
        {
            PreviewControl.Blur(20).Scale(1.2f, 1.2f, (float)Window.Current.Bounds.Width / 2, (float)Window.Current.Bounds.Height / 2).Start();
            MainControls.Offset(0, 0, duration: _transitionDuration).Start();
            Shade.Fade(1, _transitionDuration).Start();

            CameraButtons.Offset(100, 0, _transitionDuration).Start();
            CameraControlsChangeCameraButton.Offset(200, 0, _transitionDuration).Start();
            CameraControlsCaptureWhenSmilingButton.Offset(100, 0, _transitionDuration).Start();

            CameraControlsCaptureButton.Offset(0, 100, _transitionDuration).Start();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = 
                Frame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;

            Shade.IsHitTestVisible = true;
            MainControls.IsHitTestVisible = true;

            MainControlsCaptureButton.IsTabStop = true;
            MainControlsBrowseButton.IsTabStop =  true;
            MainControlsUploadButton.IsTabStop =  true;

            CameraControlsCaptureButton.IsTabStop =        false;
            CameraControlsChangeCameraButton.IsTabStop =        false;
            CameraControlsCaptureWhenSmilingButton.IsTabStop =  false;

            if (App.IsXbox())
            {
                MainControlsBrowseButton.Focus(FocusState.Keyboard);
            }
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

        private void Shade_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowCamera();
        }

        private void PhotoPreviewView_BackRequested(object sender, BackRequestedEventArgs e)
        {

            if (_camera.IsSmileDetectionEnabled)
            {
                e.Handled = true;
                _camera.IsSmileDetectionEnabled = false;
            }
            else if (!PhotoPreviewView.IsVisible)
            {
                e.Handled = true;
                ((App)(App.Current)).BackRequested -= PhotoPreviewView_BackRequested;
                HideCamera();
            }
        }

        private void CapturePhoto_Click(object sender, RoutedEventArgs e)
        {
            _camera.CapturePhotoAsync();
        }

        private async void ChangeCamera_Click(object sender, RoutedEventArgs e)
        {
            await _camera.ToggleCameraDeviceAsync();
        }

        private void CaptureWhenSmiling_Click(object sender, RoutedEventArgs e)
        {
            _camera.IsSmileDetectionEnabled = !_camera.IsSmileDetectionEnabled;
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
                //photo.ThumbnailUri = await VisionAPI.Instance.GetThumbnail(await e.File.OpenReadAsync(), e.File.DisplayName + "_thumb.jpg"); 
                await Data.Instance.SavePhotoAsync(photo);
            });

            if (App.IsXbox())
            {
                CameraControlsCaptureButton.Focus(FocusState.Keyboard);
            }
        }

        private void CaptureButtonClick(object sender, RoutedEventArgs e)
        {
            ShowCamera();
        }

        private async void UploadButtonClick(object sender, RoutedEventArgs e)
        {

            (sender as Button).IsEnabled = false;

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var files = await picker.PickMultipleFilesAsync();

            var photos = new List<PhotoData>();

            if (files != null && files.Count > 0)
            {
                var folder = await AdventureObjectStorageHelper.GetDataSaveFolder();

                foreach (var file in files)
                {
                    var newFile = file;
                    StorageFolder parent = null;

                    try
                    {
                        parent = await file.GetParentAsync();
                    } catch (Exception){ }

                    if (parent == null || parent.Path != folder.Path)
                        newFile = await file.CopyAsync(folder);

                    var photo = new PhotoData()
                    {
                        DateTime = DateTime.Now,
                        Uri = newFile.Path,
                    };

                    //photo.ThumbnailUri = await VisionAPI.Instance.GetThumbnail(await newFile.OpenReadAsync(), newFile.DisplayName + "_thumb.jpg");

                    photos.Add(photo);
                }

                await Data.Instance.SavePhotosAsync(photos);

                Frame.Navigate(typeof(AdventurePage), null);
            }
            else
            {
                (sender as Button).IsEnabled = true;
            }
        }

        private void PhotosButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdventurePage), null);
        }
    }
}
