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
            var data = await Data.Instance.GetPhotosAsync();
            for (var i = data.Count - 1; i > -1; --i)
            {
                _photos.Add(data[i]);
            }

            if (_photos.Count > 0)
            {
                BackgroundImage.ImageSource = await GetBitmapImage(_photos[new Random().Next(0, _photos.Count)].Uri);
            }
            else
            {
                // show someting
            }

            if (App.IsXbox())
            {
                ImageGridView.FindDescendant<GridViewItem>()?.Focus(FocusState.Keyboard);
            }

            base.OnNavigatedTo(e);
        }

        //private async void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    var templateRoot = args.ItemContainer.ContentTemplateRoot as Image;

        //    var photo = args.Item as PhotoData;

        //    templateRoot.Source = await GetBitmapImage(photo.ThumbnailUri);
        //}

        private async Task<BitmapImage> GetBitmapImage(string path)
        {
            var bi = new BitmapImage();

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                await bi.SetSourceAsync(await file.OpenReadAsync());
                return bi;
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

            if (App.IsXbox())
            {
                (focusItem as GridViewItem).Focus(FocusState.Keyboard);
            }

            await Data.Instance.SavePhotoAsync(photo);
        }
    }
}
