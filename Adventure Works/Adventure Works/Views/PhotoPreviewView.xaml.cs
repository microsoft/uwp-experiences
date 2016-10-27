using Adventure_Works.CognitiveServices;
using Adventure_Works.Data;
using Microsoft.Graphics.Canvas;
using Microsoft.ProjectOxford.Vision;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Animations;
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
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Adventure_Works
{
    public sealed partial class PhotoPreviewView : UserControl
    {
        private PhotoData _photo;
        private StorageFile _file;
        //private StorageFolder _folder => ApplicationData.Current.LocalFolder;

        private EffectsGenerator _effectsGenerator;
        private ICanvasImage _canvasImage;
        private EffectType _selectedEffectType;

        private bool _photoInfoInitialized;

        ObservableCollection<EffectType> _effects = new ObservableCollection<EffectType>();

        ObservableCollection<PhotoFace> _knownFaces = new ObservableCollection<PhotoFace>();
        ObservableCollection<string> _tags = new ObservableCollection<string>();

        Dictionary<PhotoFace, UIElement> _facePanels = new Dictionary<PhotoFace, UIElement>();

        public event EventHandler FinishedShowing;
        public event EventHandler FinishedHiding;

        public bool IsVisible { get; private set; } = false;

        public PhotoPreviewView()
        {
            this.InitializeComponent();
        }

        public async Task ShowAsync(PhotoData photo)
        {
            if (photo == null || photo.Uri == null)
            {
                return;
            }

            Root.Opacity = 0;
            Root.Visibility = Visibility.Visible;

            LoadingScreen.Opacity = 1;
            LoadingScreen.IsHitTestVisible = true;
            ProgressRing.IsActive = true;

            await Root.Scale(1.2f, 1.2f, (float)ActualWidth / 2, (float)ActualHeight / 2, 0).Then()
                .Fade(1).Scale(1, 1, (float)ActualWidth / 2, (float)ActualHeight / 2).StartAsync();

            _photo = photo;
            var uri = new Uri(photo.Uri);

            if (uri.IsFile)
            {
                _file = await StorageFile.GetFileFromPathAsync(photo.Uri);
            }
            else
            {
                _file = await StorageFile.CreateStreamedFileFromUriAsync("photo.jpg", uri, null);
            }

            if (_file == null)
            {
                Hide();
                return;
            }

            var stream = await _file.OpenReadAsync();
            
            _canvasImage = await CanvasBitmap.LoadAsync(ImageCanvas, stream);
            var imgBounds = _canvasImage.GetBounds(ImageCanvas);

            ImageCanvas.Height = imgBounds.Height;
            ImageCanvas.Width = imgBounds.Width;

            _selectedEffectType = EffectType.none;
            ImageCanvas.Invalidate();

            if (App.IsXbox())
            {
                DetailsButton.Focus(FocusState.Keyboard);
            }

            SetCanvasSize();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            ((App)(App.Current)).BackRequested += PhotoPreviewView_BackRequested;

            SizeChanged += PhotoPreviewView_SizeChanged;

            LoadingScreen.Fade(0, 300).Start();
            ProgressRing.IsActive = false;
            LoadingScreen.IsHitTestVisible = false;

            FinishedShowing?.Invoke(this, null);

            AnalyzeFaces();
        }

        

        public async Task<PhotoData> ShowAndWaitAsync(PhotoData photo)
        {
            await ShowAsync(photo);

            var taskSource = new TaskCompletionSource<object>();

            EventHandler completed = null;
            completed += (s, e) =>
            {
                FinishedHiding -= completed;
                taskSource.SetResult(null);
            };

            FinishedHiding += completed;

            await taskSource.Task;

            return photo;
        }
        

        public async Task Hide()
        {
            ((App)(App.Current)).BackRequested -= PhotoPreviewView_BackRequested;
            SizeChanged -= PhotoPreviewView_SizeChanged;

            IsVisible = false;

            await Root.Scale(1.2f, 1.2f, (float)ActualWidth / 2, (float)ActualHeight / 2).Fade(0).StartAsync();

            Root.Visibility = Visibility.Collapsed;

            _canvasImage.Dispose();
            _canvasImage = null;

            _photo = null;
            _file = null;

            ImageCanvas.Invalidate();

            FinishedHiding?.Invoke(this, null);

            _tags.Clear();
            _knownFaces.Clear();
            _facePanels.Clear();
            FaceCanvas.Children.Clear();

            if (Description != null)
            {
                Description.Text = "";
            }

            _effectsGenerator = null;

            FiltersButton.IsChecked = false;
            DetailsButton.IsChecked = false;

            _photoInfoInitialized = false;

            if (EffectsView != null)
            {
                EffectsView.Visibility = Visibility.Collapsed;
                _effects.Clear();
            }
            
        }

        private async Task LoadEffects()
        {
            if (_effectsGenerator == null)
            {
                var stream = await _file.OpenReadAsync();
                _effectsGenerator = await EffectsGenerator.LoadImage(stream);
            }

            if (_effects.Count == 0)
            {
                foreach (var value in Enum.GetValues(typeof(EffectType)))
                {
                    _effects.Add((EffectType)value);
                }
            }
        }

        private void PhotoPreviewView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetCanvasSize();

            List<PhotoFace> faces = _facePanels.Keys.ToList();

            foreach (var face in faces)
            {
                RemoveBoxForFace(face);
                AddBoxForFace(face);
            }
        }

        private void SetCanvasSize()
        {
            var imageRatio = ImageCanvas.Width / ImageCanvas.Height;
            var screenRatio = this.ActualWidth / this.ActualHeight;

            if (imageRatio > screenRatio)
            {
                FaceCanvas.Width = this.ActualWidth;
                FaceCanvas.Height = this.ActualWidth / imageRatio;
            }
            else
            {
                FaceCanvas.Width = this.ActualHeight * imageRatio;
                FaceCanvas.Height = this.ActualHeight;
            }
        }

        private void ImageCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            if (_canvasImage == null)
            {
                ds.Clear(Colors.Black);
                return;
            }

            var size = sender.Size;

            ds.DrawImageWithEffect(_canvasImage, new Rect(0, 0, size.Width, size.Height), _canvasImage.GetBounds(sender), _selectedEffectType);
        }

        private void Collection_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            var textBlock = templateRoot.Children[1] as TextBlock;
            textBlock.Text = ((EffectType)args.Item).ToString().ToUpper();

            args.RegisterUpdateCallback(ApplyEffectOnTemplate);
        }

        private async void ApplyEffectOnTemplate(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            var image = templateRoot.Children[0] as Image;

            BitmapImage bi = new BitmapImage();
            await bi.SetSourceAsync(await _effectsGenerator.GenerateImageWithEffect(((EffectType)args.Item)));

            image.Source = bi;
        }

        private async Task AnalyzeFaces()
        {
            IEnumerable<PhotoFace> people = null;

            if (_photo.IsProcessedForFaces)
            {
                people = _photo.People;
            }
            else
            {
                var stream = await _file.OpenReadAsync();
                people = await FaceAPI.Instance.FindPeople(stream);

                _photo.IsProcessedForFaces = true;
                _photo.People = people;
            }

            if (!IsVisible || people == null)
            {
                return;
            }

            foreach (var person in people)
            {
                if (person.Identified)
                {
                    _knownFaces.Add(person);

                    var element = AddBoxForFace(person);

                    var t = element.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await element.Fade(0, 1000, 1000).StartAsync();
                        RemoveBoxForFace(person);
                    });
                }
                else
                {
                    AddBoxForFace(person);
                }
            }
        }

        private UIElement AddBoxForFace(PhotoFace face)
        {
            var scale =  FaceCanvas.ActualWidth / ImageCanvas.Width;

            var width = face.Rect.Width * scale;
            var height = face.Rect.Height * scale;
            var top = face.Rect.Top * scale;
            var left = face.Rect.Left * scale;

            if (face.Identified)
            {
                var border = new Border()
                {
                    Width = width > 200 ? width : double.NaN,
                };

                border.SetValue(Canvas.TopProperty, (top + height));
                border.SetValue(Canvas.LeftProperty, left);

                var background = new Border()
                {
                    Background = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0)),
                    Padding = new Thickness(8),
                    CornerRadius = new CornerRadius(10),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                border.Child = background;

                var textblock = new TextBlock()
                {
                    Text = face.Name,
                    FontSize = 12
                };

                background.Child = textblock;

                FaceCanvas.Children.Add(border);
                if (!_facePanels.ContainsKey(face))
                    _facePanels.Add(face, border);

                return border;
            }
            else
            {
                var panel = new StackPanel()
                {
                    //Width = face.Rect.Width
                };

                panel.SetValue(Canvas.TopProperty, top);
                panel.SetValue(Canvas.LeftProperty, left);

                var rect = new Rectangle()
                {
                    Height = height,
                    Width = width,
                    Stroke = (App.Current.Resources["BrandColor"]) as SolidColorBrush,
                    StrokeThickness = 4,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                var nameBox = new AutoSuggestBox()
                {
                    Text = face.Identified ? face.Name : "",
                    PlaceholderText = !face.Identified ? "who is this?" : "",
                    Width = width > 200 ? width : 200,
                    QueryIcon = new SymbolIcon(Symbol.Go),
                    DataContext = face
                };

                nameBox.TextChanged += NameBox_TextChanged;
                nameBox.QuerySubmitted += NameBox_QuerySubmitted;

                panel.Children.Add(rect);
                panel.Children.Add(nameBox);
                FaceCanvas.Children.Add(panel);

                _facePanels.Add(face, panel);

                return panel;
            }
        }

        private void RemoveBoxForFace(PhotoFace face)
        {
            if (face == null)
                return;

            UIElement element;

            _facePanels.TryGetValue(face, out element);

            if (element != null)
            {
                FaceCanvas.Children.Remove(element);
                _facePanels.Remove(face);
            }
        }

        private async Task AnalyzeImage()
        {
            if (_photoInfoInitialized)
            {
                return;
            }

            _photoInfoInitialized = true;

            if (_photo.IsAnalyzed)
            {
                foreach (var tag in _photo.Tags)
                {
                    _tags.Add(tag);
                }

                Description.Text = _photo.Description;
            }
            else
            {
                MetadataProgress.Visibility = Visibility.Visible;
                var stream = await _file.OpenReadAsync();

                var imageResults = await VisionAPI.Instance.AnalyzePhoto(stream);

                if (imageResults == null)
                    return;

                foreach (var tag in imageResults.Tags)
                {
                    _tags.Add(tag.Name);
                }

                _photo.Tags = _tags.ToList();
                _photo.Description = imageResults.Description.Captions.First().Text;
                _photo.IsAnalyzed = true;

                Description.Text = _photo.Description;

                MetadataProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void NameBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            sender.IsEnabled = false;

            PhotoFace person = sender.DataContext as PhotoFace;


            if (sender.Text != person.Name)
            {
                var stream = await _file.OpenReadAsync();
                PhotoFace newPerson = new PhotoFace();
                newPerson.Name = sender.Text;
                newPerson.Rect = person.Rect;
                newPerson.Identified = true;

                var knownPerson = FaceAPI.Instance.KnownPeople.Where(p => p.Name == sender.Text).FirstOrDefault();
                if (knownPerson != null)
                {
                    newPerson.PersonId = knownPerson.PersonId;
                }

                await FaceAPI.Instance.AddImageForPerson(newPerson, stream);


                var newList = _photo.People.ToList();
                newList.Remove(person);
                newList.Add(newPerson);

                _photo.People = newList;

                RemoveBoxForFace(person);
                _knownFaces.Add(newPerson);
            }

            sender.IsEnabled = true;
        }

        private void NameBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = FaceAPI.Instance.KnownPeople.Where(p => p.Name.ToLower().Contains(sender.Text.ToLower())).Select(p => p.Name);
            }
        }

        private void Collection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            _selectedEffectType = (EffectType)e.AddedItems.FirstOrDefault();
            ImageCanvas.Invalidate();
        }

        private void PhotoPreviewView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Hide();
        }
        
        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AddBoxForFace((sender as FrameworkElement).DataContext as PhotoFace);
        }

        private void FaceItemsControl_GotFocus(object sender, RoutedEventArgs e)
        {
            AddBoxForFace((e.OriginalSource as FrameworkElement).DataContext as PhotoFace);
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            RemoveBoxForFace((sender as FrameworkElement).DataContext as PhotoFace);
        }

        private void FaceItemsControl_LostFocus(object sender, RoutedEventArgs e)
        {
            RemoveBoxForFace((e.OriginalSource as FrameworkElement).DataContext as PhotoFace);
        }

        private async void FiltersChecked(object sender, RoutedEventArgs e)
        {
            DetailsButton.IsChecked = false;

            FindName("EffectsView");
            await LoadEffects();
            EffectsView.Visibility = Visibility.Visible;

            if (App.IsXbox())
            {
                var item = EffectsView.FindDescendant<GridViewItem>();
                item?.Focus(FocusState.Keyboard);
            }
        }

        private void FiltersUnchecked(object sender, RoutedEventArgs e)
        {
            FindName("EffectsView");
            EffectsView.Visibility = Visibility.Collapsed;
        }

        private async void DetailsChecked(object sender, RoutedEventArgs e)
        {
            FiltersButton.IsChecked = false;

            FindName("MetadataGrid");
            MetadataProgress.Visibility = Visibility.Collapsed;
            MetadataGrid.Visibility = Visibility.Visible;

            await AnalyzeImage();
        }

        private void DetailsUnchecked(object sender, RoutedEventArgs e)
        {
            FindName("MetadataGrid");
            MetadataGrid.Visibility = Visibility.Collapsed;
        }

        private async void FocusableItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            var photoFace = item.DataContext as PhotoFace;

            var dialog = new Windows.UI.Popups.MessageDialog(
                $"Do you want to cut all ties with {photoFace.Name}?",
                "Cut'em");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
            {
                await FaceAPI.Instance.DeletePersonAsync(photoFace.PersonId);
                await DataProvider.Instance.ClearSavedFaces();
            }

        }

        private async void FacebookShareClicked(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            ShareButtons.Fade(0, 200).Start();
            ShareProgress.IsActive = true;
            ShareProgress.Visibility = Visibility.Visible;

            await Identity.Instance.SharePhotoAsync(IdentityProvider.Facebook, await _file.OpenReadAsync());
            ShareFlyout.Hide();
            (sender as Button).IsEnabled = true;
        }


        private async void TwitterShareClicked(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            ShareButtons.Fade(0, 200).Start();
            ShareProgress.IsActive = true;
            ShareProgress.Visibility = Visibility.Visible;

            await Identity.Instance.SharePhotoAsync(IdentityProvider.Twitter, await _file.OpenReadAsync());
            ShareFlyout.Hide();
            (sender as Button).IsEnabled = true;
        }

        private void ShareFlyout_Closed(object sender, object e)
        {
            ShareButtons.Opacity = 1;
            ShareProgress.IsActive = false;
            ShareProgress.Visibility = Visibility.Collapsed;
        }
    }
}
