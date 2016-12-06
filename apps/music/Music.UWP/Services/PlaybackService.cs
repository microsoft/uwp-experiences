using Music.PCL;
using Music.PCL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Music.Services
{
    public class PlaybackService : MainViewModel
    {
        static PlaybackService instance;

        public const string MediaItemIdKey = "mediaItemId";

        private ObservableCollection<Song> _songCollection;

        public override ObservableCollection<Song> SongCollection
        {
            get
            {
                return _songCollection;
            }

            set
            {
                if (_songCollection == value) return;

                if (_songCollection != null)
                {
                    _songCollection.CollectionChanged -= PlaylistCollection_CollectionChanged;

                    if (_playlist != null)
                    {
                        _playlist.Items.Clear();
                    }
                }

                _songCollection = value;

                if (_songCollection != null)
                {
                    _songCollection.CollectionChanged += PlaylistCollection_CollectionChanged;
                    foreach (var song in _songCollection)
                    {
                        _playlist.Items.Add(ToPlaybackItem(song));
                        Debug.WriteLine("song added again" + (song as Song).Title);

                    }
                }

                RaisePropertyChanged();
            }
        }

        public override Song CurrentItem
        {
            get { return _currentItemIndex == -1 ? null : _songCollection[_currentItemIndex]; }
            set
            {
                if (value == null)
                {
                    CurrentItemIndex = -1;
                    return;
                }

                if (_currentItemIndex == -1 || _songCollection[_currentItemIndex] != value)
                {
                    CurrentItemIndex = _songCollection.IndexOf(value);
                }
            }
        }

        int _currentItemIndex = -1;

        public override int CurrentItemIndex
        {
            get
            {
                return _currentItemIndex;
            }
            set
            {
                if (value < 0)
                {
                    return;
                }

                if (_currentItemIndex != value)
                {
                    // Clamp invalid values
                    var min = -1;
                    var max = _playlist.Items.Count - 1;
                    _currentItemIndex = (value < min) ? min : (value > max) ? max : value;

                    if (_currentItemIndex >= 0 && _currentItemIndex != _playlist.CurrentItemIndex)
                        _playlist.MoveTo((uint)_currentItemIndex);

                    RaisePropertyChanged("CurrentItemIndex");
                    RaisePropertyChanged("CurrentItem");
                    RaisePropertyChanged("CanGoBack");
                    RaisePropertyChanged("CanGoForward");
                }
            }
        }

        public bool CanGoForward => _currentItemIndex < _playlist.Items.Count - 1;

        public bool CanGoBack => _currentItemIndex > 0;


        public static PlaybackService Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlaybackService();

                return instance;
            }
        }

        public static void CleanUp()
        {
            if (instance != null)
            {
                instance._player.PlaybackSession.PositionChanged -= instance.PlaybackSession_PositionChanged;
                instance._player.PlaybackSession.PlaybackStateChanged -= instance.PlaybackSession_PlaybackStateChanged;
                instance._player.Dispose();
                instance._player = null;

                instance._playlist.CurrentItemChanged -= instance._playlist_CurrentItemChanged;
                instance._playlist = null;

                instance = null;
            }
        }

        private MediaPlayer _player { get; set; }

        private MediaPlaybackList _playlist { get; set; }

        public PlaybackService()
        {
            // Create the player instance
            _player = new MediaPlayer();
            _player.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            _player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            _playlist = new MediaPlaybackList();
            _playlist.CurrentItemChanged += _playlist_CurrentItemChanged;
            _player.Source = _playlist;
            _player.Play();

            Dispatcher = new DispatcherWrapper(CoreApplication.MainView.CoreWindow.Dispatcher);
        }

        public void TogglePlayPause()
        {
            if (_player.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                _player.Play();
            }
            else if (_player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                _player.Pause();
            }
        }

        public void Next()
        {
            if (CanGoForward)
                CurrentItemIndex++;
        }

        public void Previous()
        {
            if (CanGoBack)
                CurrentItemIndex--;
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            switch(sender.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    PlayerState = PlaybackState.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    PlayerState = PlaybackState.Paused;
                    break;
                default:
                    PlayerState = PlaybackState.Other;
                    Position = 0;
                    Duration = 0;
                    break;
            }

            if (CloudService.Instance.State == ServiceState.Connected &&
                CloudService.Instance.ParticipationType == ParticipationType.Host)
            {
                CloudService.Instance.SendStatusUpdate(GetStatus());
            }
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            Position = (int)sender.Position.TotalSeconds;
            Duration = (int)sender.NaturalDuration.TotalSeconds;

            if (CloudService.Instance.State == ServiceState.Connected &&
                CloudService.Instance.ParticipationType == ParticipationType.Host)
            {
                CloudService.Instance.SendStatusUpdate(GetStatus());
            }
        }

        private PartyStatus GetStatus()
        {
            return new PartyStatus()
            {
                State = PlayerState,
                TrackIndex = _currentItemIndex,
                Duration = Duration,
                Progress = Position
            };
        }

        private void _playlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (CoreApplication.MainView.CoreWindow != null)
            {
                var t = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    var item = args.NewItem;

                    if (item == null)
                    {
                        CurrentItem = null;
                    }
                    else
                    {
                        var index = _playlist.Items.IndexOf(item);
                        CurrentItem = _songCollection[index];
                    }
                });
            }
        }

        private MediaPlaybackItem ToPlaybackItem(Song song)
        {
            var source = MediaSource.CreateFromUri(new Uri(song.StreamUrl));

            var playbackItem = new MediaPlaybackItem(source);
            
            var displayProperties = playbackItem.GetDisplayProperties();
            displayProperties.Type = Windows.Media.MediaPlaybackType.Music;
            displayProperties.MusicProperties.Title = song.Title;
            displayProperties.MusicProperties.Artist = song.Artist;
            displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(song.AlbumArt));

            playbackItem.ApplyDisplayProperties(displayProperties);
            
            source.CustomProperties[MediaItemIdKey] = song.ItemId;

            return playbackItem;
        }

        private void PlaylistCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (var song in e.NewItems)
                        {
                            _playlist.Items.Add(ToPlaybackItem(song as Song));
                            Debug.WriteLine("song added " + (song as Song).Title);
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    var item = _playlist.Items[e.OldStartingIndex];
                    _playlist.Items.RemoveAt(e.OldStartingIndex);
                    _playlist.Items.Insert(e.NewStartingIndex, item);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex > -1)
                    {
                        _playlist.Items.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.NewItems.Count; ++i)
                    {
                        var oldItem = e.OldItems[i] as Song;
                        var newItem = e.NewItems[i] as Song;

                        var pItem = _playlist.Items.SingleOrDefault(pi => (string)pi.Source.CustomProperties[MediaItemIdKey] == oldItem.ItemId);
                        if (pItem != null)
                        {
                            var index = _playlist.Items.IndexOf(pItem);
                            _playlist.Items.RemoveAt(index);
                            _playlist.Items.Insert(index, ToPlaybackItem(newItem));
                        }

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    _playlist.Items.Clear();
                    break;
            }
        }
    }
}
