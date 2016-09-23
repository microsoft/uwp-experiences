using Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Music.Helpers;
using Music.PCL;
using Music.PCL.Models;
using Music.PCL.Services;
using Music.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Music
{
    public sealed partial class MainPage : Page
    {
        //PlaybackService _playerService => PlaybackService.Instance;
        MainViewModel _viewModel;
        DataService _dataService => DataService.Instance;
        PlaybackService _playerService => _viewModel as PlaybackService;


        int _imageIndex = 0;

        DispatcherTimer _timer;
        bool _timerFirstRun = true;
        bool _faded = false;

        bool _isHost = false;
        bool _isClient => !_isHost;

        bool _isFullView = true;
        bool _addingSongs = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null && e.Parameter is bool && !(bool)e.Parameter)
            {
                _isHost = false;
                _viewModel = new ClientViewModel();
                _viewModel.Dispatcher = new DispatcherWrapper(Dispatcher);
                //init as client
            }
            else
            {
                _isHost = true;
                _viewModel = PlaybackService.Instance;
                if (_viewModel.SongCollection == null)
                    _viewModel.SongCollection = DataService.Instance.Playlist;
                var t = DataService.Instance.InitDataServiceAsHost();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (_isHost)
            {
                PlaybackService.CleanUp();
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            DataService.Instance.Dispatcher = new DispatcherWrapper(Dispatcher);
            BackgroundContainer.Blur(20, 0).Start();

            if (_isHost)
            {
                if (App.IsXbox())
                {
                    PlayPause.Focus(FocusState.Keyboard);
                }

                this.PointerMoved += MainPage_PointerMoved;
                this.KeyDown += MainPage_KeyDown;
                _playerService.PropertyChanged += _dataService_PropertyChanged;

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(10);
                _timer.Tick += _timer_Tick;
                _timer.Start();
            }
            else
            {

                AddButton.Focus(FocusState.Pointer);
                UserSongCollection.ItemsSource = (_viewModel as ClientViewModel).UserSongs;
                var t = (_viewModel as ClientViewModel).LoadUserSongs();

                this.SizeChanged += MainPage_SizeChanged;
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DataService.Instance.Dispatcher = null;

            if (_isHost)
            {
                this.PointerMoved -= MainPage_PointerMoved;
                this.KeyDown -= MainPage_KeyDown;
                _playerService.PropertyChanged -= _dataService_PropertyChanged;

                _timer.Tick -= _timer_Tick;
                _timer = null;
            }
            else
            {
                this.SizeChanged -= MainPage_SizeChanged;

            }
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            _timer.Start();
            
            if (e.Key == Windows.System.VirtualKey.GamepadLeftShoulder)
                _playerService.Previous();
            else if (e.Key == Windows.System.VirtualKey.GamepadRightShoulder)
                _playerService.Next();
            else
                FadeIn();
        }

        private void MainPage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            _timer.Start();
            FadeIn();
        }

        private void _timer_Tick(object sender, object e)
        {
            if (_timerFirstRun)
            {
                _timerFirstRun = false;
                _timer.Interval = TimeSpan.FromSeconds(10);
            }

            FadeAway();
        }

        private void FadeAway(bool ignoreFaded = false)
        {
            if (_faded && !ignoreFaded) return;

            _faded = true;
            var items = SongTimeline.FindChildren<TimelineItem>();

            for (var i = 0; i < items.Count(); i++)
            {
                if (!_faded) return;
                if (i != SongTimeline.CurrentItemIndex)
                {
                    items.ElementAt(i).Fade(0).Start();
                }
            }

            var item = items.ElementAt(SongTimeline.CurrentItemIndex);

            item.Fade(1).Scale(1.2f, 1.2f, (float)item.DesiredSize.Width / 2, (float)item.DesiredSize.Height / 2).Start();
        }

        private void FadeIn()
        {
            if (!_faded) return;
            _faded = false;
            var items = SongTimeline.FindChildren<TimelineItem>();

            foreach (var item in items)
            {
                if (_faded) return;
                item.Fade(1).Scale(1, 1, (float)item.DesiredSize.Width / 2, (float)item.DesiredSize.Height / 2).Start();
            }
        }

        private void _dataService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentItem" && _faded)
            {
                FadeAway(true);
            }
        }

        private Symbol StateToSymbol(PlaybackState state)
        {
            switch (state)
            {
                case PlaybackState.Playing:
                    return Symbol.Pause;
                case PlaybackState.Paused:
                    return Symbol.Play;
                default:
                    return Symbol.Play;
            }
        }

        private string PlaybackServiceToPosition(int dummy)
        {

            if (_viewModel.PlayerState == PlaybackState.Other)
            {
                return "-- / --";
            }
            else
            {
                return $"{_viewModel.Position} / {_viewModel.Duration}";
            }
        }

        private Visibility ObjectToVisibility(object obj)
        {
            return obj == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            _playerService.TogglePlayPause();
        }

        private void TimelineItem_ItemGotFocus(object sender, EventArgs e)
        {
            var song = (sender as TimelineItem).DataContext as Song;
            UpdateBackground(song.AlbumArtLarge);

            if (CloudService.Instance.ParticipationType == ParticipationType.Host)
                _viewModel.CurrentItemIndex = SongTimeline.TimelinePanel.ItemIndex;
            
        }

        public async Task UpdateBackground(string uri)
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

        private void PreviousClicked(object sender, RoutedEventArgs e)
        {
            _playerService.Previous();
        }

        private void NextClicked(object sender, RoutedEventArgs e)
        {
            _playerService.Next();
        }

        private string GetPassedTime(int position)
        {
            var t = TimeSpan.FromSeconds(position);
            return t.ToString(@"%m\:ss");
        }

        private string GetRemainingTime(int position)
        {
            var t = TimeSpan.FromSeconds(_viewModel.Duration - position);
            return t.ToString(@"\-%m\:ss");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleAddSongs();
        }

        private void UpvoteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DownvoteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleViewButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleView();
        }

        private void AddOrLikeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async Task ToggleAddSongs()
        {
            _addingSongs = !_addingSongs;

            if (_addingSongs)
            {
                if (_isFullView)
                    ToggleView();

                UserSongCollection.Opacity = 0;
                UserSongCollection.Visibility = Visibility.Visible;

                UserSongCollection.Offset(0, (float)Window.Current.Bounds.Height, 0).Then()
                    .Fade(1, 0).Offset(0, 0).Start();
                CurrentPlaylistList.Offset(0, (float)-Window.Current.Bounds.Height).Then().Fade(0,0).Start();
                MainAddSymbol.Symbol = Symbol.List;
                MiniAddSymbol.Symbol = Symbol.List;
            }
            else
            {
                MainAddSymbol.Symbol = Symbol.Add;
                MiniAddSymbol.Symbol = Symbol.Add;

                CurrentPlaylistList.Offset(0, (float)-Window.Current.Bounds.Height, 0).Then()
                    .Fade(1, 0).Offset(0, 0).Start();

                await UserSongCollection.Offset(0, (float)Window.Current.Bounds.Height).Then()
                    .Fade(0, 0).StartAsync();

                UserSongCollection.Visibility = Visibility.Collapsed;
            }

            
        }

        private void ToggleView()
        {
            _isFullView = !_isFullView;

            if (_isFullView)
            {
                TopBackground.Visibility = Visibility.Collapsed;
                TopGrid.Margin = new Thickness(-48, -27, -48, 0);
                ToggleViewSymbol.Visibility = Visibility.Visible;
                ToggleViewImage.Visibility = Visibility.Collapsed;
                ArtColumn.Width = new GridLength(1, GridUnitType.Star);
                DetailsColumn.Width = new GridLength(0);
                SongList.Visibility = Visibility.Collapsed;
                TitleGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                TitleGrid.Width = Double.NaN;
                TitleGrid.Margin = new Thickness(0);
                MetadataPanel.Visibility = Visibility.Visible;
                MiddleGrid.Margin = new Thickness(-48, 0, -48, 60);
                NowPlayingRow.Height = new GridLength(0);

                if (_addingSongs)
                    ToggleAddSongs();
            }
            else
            {
                GoToListView();
            }
        }

        private void GoToListView()
        {
            if (this.DesiredSize.Width > 900)
            {
                NowPlayingGrid.Visibility = Visibility.Collapsed;
                MetadataPanel.Visibility = Visibility.Visible;
                MiddleGrid.Margin = new Thickness(-48, 52, -48, 60);
                NowPlayingRow.Height = new GridLength(0);
                TopBackground.Visibility = Visibility.Visible;
                TopGrid.Margin = new Thickness(352, -27, -48, 0);
                ToggleViewSymbol.Visibility = Visibility.Collapsed;
                ToggleViewImage.Visibility = Visibility.Visible;
                ArtColumn.Width = new GridLength(400);
                DetailsColumn.Width = new GridLength(1, GridUnitType.Star);
                SongList.Visibility = Visibility.Visible;
                TitleGrid.HorizontalAlignment = HorizontalAlignment.Left;
                TitleGrid.Width = 400;
                TitleGrid.Margin = new Thickness(-48, 0, 0, 0);
            }
            else
            {
                NowPlayingGrid.Visibility = Visibility.Visible;
                NowPlayingRow.Height = new GridLength(120);
                MiddleGrid.Margin = new Thickness(-48, 52, -48, -27);
                MetadataPanel.Visibility = Visibility.Collapsed;
                TopBackground.Visibility = Visibility.Collapsed;
                TopGrid.Margin = new Thickness(-48, -27, -48, 0);
                ToggleViewSymbol.Visibility = Visibility.Collapsed;
                ToggleViewImage.Visibility = Visibility.Visible;
                ArtColumn.Width = new GridLength(0);
                DetailsColumn.Width = new GridLength(1, GridUnitType.Star);
                SongList.Visibility = Visibility.Visible;
                TitleGrid.HorizontalAlignment = HorizontalAlignment.Left;
                TitleGrid.Width = 400;
                TitleGrid.Margin = new Thickness(-48, 0, 0, 0);
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isFullView) return;

            if ((e.NewSize.Width >= 900 && e.PreviousSize.Width < 900) ||
                (e.NewSize.Width < 900 && e.PreviousSize.Width >= 900))
            {
                GoToListView();
            }
        }

        private void SongList_AddClicked(object sender, Song e)
        {
            (_viewModel as ClientViewModel).AddSong(e);
        }
    }
}
