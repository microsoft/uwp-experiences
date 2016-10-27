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
using Adventure_Works.Data;
using Adventure_Works.Rome;
using Windows.Devices.Geolocation;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml.Controls.Maps;

namespace Adventure_Works
{
    public sealed partial class MainPage : Page
    {
        Adventure CurrentAdventure;
        AdventureRemoteSystem _system;

        public MainPage()
        {
            this.InitializeComponent();
            Map.MapServiceToken = Keys.MapServiceToken;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {

            // Set the map location.
            var locationTask = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Map.LandmarksVisible = true;
                LoadMapElements();
                var currentLocation = await Maps.GetCurrentLocationAsync();

                if (currentLocation != null)
                {
                    var icon = new MapIcon();
                    icon.Location = currentLocation;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
                    icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Square44x44Logo.targetsize-30.png"));
                    
                    Map.MapElements.Add(icon);
                }
            });

            CurrentAdventure = await DataProvider.Instance.GetCurrentAdventure();
            List<UIElement> elementsToShow = new List<UIElement>();

            if (await DataProvider.Instance.GetCurrentAdventure() == null)
            {
                elementsToShow.Add(MainControlsNewAdventureButtonContainer);
                elementsToShow.Add(MainControlsViewOldAdventuresButtonContainer);
            }
            else
            {
                elementsToShow.Add(MainControlsViewOldAdventuresButtonContainer);
                elementsToShow.Add(MainControlsCaptureButtonContainer);
                elementsToShow.Add(MainControlsUploadButtonContainer);
                elementsToShow.Add(MainControlsBrowseButtonContainer);
            }

            for (var i = 0; i< elementsToShow.Count; i++)
            {
                var elem = elementsToShow[i];
                elem.Opacity = 0;
                elem.Visibility = Visibility.Visible;
                elem.Offset(0, 20, 0).Then().Offset().Fade(1).SetDuration(200).SetDelay(i * 100).Start();
            }

            if (App.IsXbox())
            {
                MainControlsCaptureButton.Focus(FocusState.Keyboard);
                return;
            }

            Task.Run<List<AdventureRemoteSystem>>(async () =>
            {
                await Task.Delay(2000);
                return await ConnectedService.Instance.FindAllRemoteSystemsHostingAsync();
            }).AsAsyncOperation<List<AdventureRemoteSystem>>().Completed = (s, args) =>
            {
                var systems = s.GetResults();
                if (systems.Count > 0)
                {
                    FindName("RemoteSystemContainer");

                    _system = systems.First();

                    DeviceTypeTextBlock.Text = _system.RemoteSystem.Kind.ToString();
                    RemoteSystemContainer.Opacity = 0;
                    RemoteSystemContainer.Visibility = Visibility.Visible;
                    RemoteSystemContainer.Scale(0.8f, 0.8f, (float)ActualWidth / 2, (float)ActualHeight / 2, 0)
                                         .Then().Scale(1, 1, (float)ActualWidth / 2, (float)ActualHeight / 2).Fade(1).Start();
                }
            };
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private async void LoadMapElements()
        {
            var adventures = await DataProvider.Instance.GetFriendsAdventures();

            foreach (var adventure in adventures.OrderBy(a => a.Location.Longitude))
            {
                var point = new Geopoint(adventure.Location);
                var ellipse = new Ellipse()
                {
                    Height = 50,
                    Width = 50,
                    Stroke = App.Current.Resources["BrandColor"] as SolidColorBrush,
                    StrokeThickness = 2,
                    Fill = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri(adventure.User.Image))
                    },
                };

                var button = new Button();
                button.DataContext = adventure;
                button.Template = App.Current.Resources["MapButtonStyle"] as ControlTemplate;
                button.UseSystemFocusVisuals = false;
                button.Content = ellipse;
                button.Click += MapButtonClicked;

                Map.Children.Add(button);
                MapControl.SetLocation(button, point);
                MapControl.SetNormalizedAnchorPoint(button, new Point(0.5, 0.5));
            }

            var box = new GeoboundingBox(
                new BasicGeoposition()
                {
                    Latitude = adventures.OrderBy(a => a.Location.Latitude).Select(a => a.Location.Latitude).Last(),
                    Longitude = adventures.OrderBy(a => a.Location.Longitude).Select(a => a.Location.Longitude).First()
                },
                new BasicGeoposition()
                {
                    Latitude = adventures.OrderBy(a => a.Location.Latitude).Select(a => a.Location.Latitude).First(),
                    Longitude = adventures.OrderBy(a => a.Location.Longitude).Select(a => a.Location.Longitude).Last()
                });

            await Map.TrySetViewBoundsAsync(box, new Thickness(40), MapAnimationKind.Default);
        }

        private void MapButtonClicked(object sender, RoutedEventArgs e)
        {
            var adventure = (sender as Button).DataContext as Adventure;

            Frame.Navigate(typeof(AdventurePage), adventure.Id.ToString());
        }

        private void CaptureButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CapturePage));
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

                    if ((await file.GetParentAsync()).Path != folder.Path)
                        newFile = await file.CopyAsync(folder);

                    var photo = new PhotoData()
                    {
                        DateTime = DateTime.Now,
                        Uri = newFile.Path,
                    };

                    //photo.ThumbnailUri = await VisionAPI.Instance.GetThumbnail(await newFile.OpenReadAsync(), newFile.DisplayName + "_thumb.jpg");

                    photos.Add(photo);
                }

                //await Data.Instance.SavePhotosAsync(photos);

                //Frame.Navigate(typeof(AdventurePage), null);
            }
            else
            {
                (sender as Button).IsEnabled = true;
            }
        }

        private void PhotosButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdventurePage), CurrentAdventure.Id.ToString());
        }

        private void MainControlsViewOldAdventuresButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {

            // hide MapToolbar controls
            var toolbar = Map.FindDescendant<StackPanel>();
            if (toolbar != null)
            {
                toolbar.Visibility = Visibility.Collapsed;
            }
        }

        private async void RemoteSystemConnectButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            var response = await ConnectedService.Instance.ConnectToSystem(_system as AdventureRemoteSystem);

            if (response != null)
            {
                Frame.Navigate(typeof(SlideshowClientPage), response);
            }
            else
            {
                (sender as Button).IsEnabled = true;
            }
        }

        private void RemoteSystemCancelButton_Click(object sender, RoutedEventArgs e)
        {
            RemoteSystemContainer.Fade(0).Scale(0.8f, 0.8f, (float)ActualWidth / 2, (float)ActualHeight / 2).SetDurationForAll(200).Start();
        }

        private void Map_ActualCameraChanged(MapControl sender, MapActualCameraChangedEventArgs args)
        {
            var buttons = new Dictionary<Point, Button>();

            foreach (var child in Map.Children)
            {

                var btn = child as Button;
                Point point;
                try
                {
                    point = btn.TransformToVisual(Map).TransformPoint(new Point());
                }
                catch (Exception)
                {
                    return;
                }

                if (point.X > 0 && point.X <= Map.ActualWidth &&
                    point.Y > 0 && point.Y <= Map.ActualHeight)
                {
                    btn.IsTabStop = true;
                    buttons.Add(point, btn);
                }
                else
                {
                    btn.IsTabStop = false;
                }
            }

            Button previosBtn = null;
            var orderedButtons = buttons.OrderBy(b => b.Key.X);

            foreach (var point in orderedButtons)
            {
                var button = point.Value;

                button.XYFocusUp = button;
                button.XYFocusRight = button;
                button.XYFocusLeft = previosBtn != null ? previosBtn : button;
                button.XYFocusDown = MainControlsViewOldAdventuresButton;

                if (previosBtn != null)
                {
                    previosBtn.XYFocusRight = button;
                }

                previosBtn = button;
            }

            if (orderedButtons.Count() > 1)
            {
                orderedButtons.Last().Value.XYFocusRight = orderedButtons.First().Value;
                orderedButtons.First().Value.XYFocusLeft = orderedButtons.Last().Value;
            }
        }
    }
}
