using Adventure_Works.Data;
using Adventure_Works.Rome;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Adventure_Works
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdventurePage : Page
    {

        ObservableCollection<PhotoData> _photos = new ObservableCollection<PhotoData>();

        Adventure _adventure;
        Flyout _flyout;

        public AdventurePage()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            var id = e.Parameter;
            _adventure = await DataProvider.Instance.GetAdventure(id as string);

            AdventureNameText.Text = _adventure.Name.ToUpper();
            if (_adventure.User != null)
            {
                UserImage.ImageSource = new BitmapImage(new Uri(_adventure.User.Image));
            }

            for (var i = _adventure.Photos.Count - 1; i > -1; --i)
            {
                _photos.Add(_adventure.Photos[i]);
            }

            if (_photos.Count > 0)
            {
                var randomPhoto = _photos[new Random().Next(0, _photos.Count)];
                BackgroundImage.ImageSource = await GetBitmapImage(randomPhoto.Uri, randomPhoto.IsLocal);
            }
            else
            {
                // show someting
            }

            if (App.IsXbox())
            {
                ImageGridView.FindDescendant<GridViewItem>()?.Focus(FocusState.Keyboard);
            }
            else if (ConnectedService.Instance.Rome != null)
            {
                if (App.IsMobile())
                {
                    FindName("BottomConnectButton");
                    BottomRemoteSystemsList.ItemsSource = Rome.ConnectedService.Instance.Rome.AvailableRemoteSystems;
                    _flyout = BottomFlyout;
                }
                else
                {
                    FindName("ConnectButton");
                    RemoteSystemsList.ItemsSource = Rome.ConnectedService.Instance.Rome.AvailableRemoteSystems;
                    _flyout = Flyout;
                }
            }

            base.OnNavigatedTo(e);
        }

        private async Task<BitmapImage> GetBitmapImage(string path, bool isLocal)
        {
            try
            {
                if (isLocal)
                {
                    var bi = new BitmapImage();
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    await bi.SetSourceAsync(await file.OpenReadAsync());
                    return bi;
                }
                else
                {
                    return new BitmapImage(new Uri(path));
                }
                
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var photo = e.ClickedItem as PhotoData;

            object focusItem = null;

            if (App.IsXbox())
            {
                focusItem = FocusManager.GetFocusedElement();
            }

            await PhotoPreviewView.ShowAndWaitAsync(photo);

            // quick way to refresh binding
            var index = _photos.IndexOf(photo);
            _photos.RemoveAt(index);
            _photos.Insert(index, photo);


            if (App.IsXbox())
            {
                (focusItem as GridViewItem).Focus(FocusState.Keyboard);
            }

            await DataProvider.Instance.SavePhotoAsync(photo);
        }

        private void Slideshow_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SlideshowPage), _adventure.Id.ToString());
        }
        
        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            _flyout.Hide();
            var success = await ConnectedService.Instance.ConnectToSystem(e.ClickedItem as AdventureRemoteSystem, "slideshow/" + _adventure.Id);

            if (success)
            {
                Frame.Navigate(typeof(SlideshowClientPage), _adventure.Id.ToString());
            }
        }

        private void ImageGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var range = 5 * (180 + 16);
            if (e.NewSize.Width > range)
            {
                DescriptionText.Width = range - 16;
            }
            else
            {
                DescriptionText.Width = (int)(e.NewSize.Width / 196) * 196 - 16;
            }
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            DataProvider.Instance.ClearSavedFaces();
        }
    }
}
