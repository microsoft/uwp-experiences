using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Music.PCL.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Music.PCL.Services
{
    public class DataService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IDispatcher _dispatcher;

        public IDispatcher Dispatcher {
            get
            {
                return _dispatcher;
            }
            set
            {
                _dispatcher = value;
                Users.Dispatcher = value;
                Playlist.Dispatcher = value;
            }
        }

        private static DataService _instance;

        public static DataService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataService();
                }

                return _instance;
            }
        }

        public MTObservableCollection<User> Users { get; set; } = new MTObservableCollection<User>();

        public MTObservableCollection<Song> Playlist { get; set; } = new MTObservableCollection<Song>();

        private string _partyCode;

        public string PartyCode
        {
            get { return _partyCode; }
            set
            {
                _partyCode = value;
                RaisePropertyChanged();
            }
        }

        private DataService()
        {
            CloudService.Instance.PartyEnded += Instance_PartyEnded;
            CloudService.Instance.SomeoneConnected += Instance_SomeoneConnected;
            CloudService.Instance.SomeoneDisconnected += Instance_SomeoneDisconnected;
            CloudService.Instance.SongAdded += Instance_SongAdded;
            CloudService.Instance.SongRequested += Instance_SongRequested;

            if (CloudService.Instance.User != null)
                Users.Add(CloudService.Instance.User);
        }

        public async Task InitDataServiceAsHost(bool reset = false)
        {
            if (Playlist.Count == 0 || reset)
            {
                var tracks = await SC.API.Instance.GetPlaylistTracks(Keys.SC_STARTING_PLAYLIST); // ADX playlist

                Playlist.Clear();

                foreach (var track in tracks)
                {
                    Playlist.Add(SC.API.GetSong(track));
                }
            }

            if (string.IsNullOrEmpty(PartyCode) || reset)
            {
                PartyCode = await CloudService.Instance.StartParty(Playlist.ToList(), null);
            }
        }

        public async Task<bool> InitDataServiceAsClient(string code)
        {
            if (CloudService.Instance.State != ServiceState.Connected)
                return false;

            var result = await CloudService.Instance.JoinParty(code);

            if (!result.Success) return false;

            foreach (var song in result.Songs)
            {
                Playlist.Add(song);
            }

            foreach (var user in result.Users)
            {
                Users.Add(user);
            }

            PartyCode = code;

            return true;
        }

        private void Instance_SongRequested(object sender, Song e)
        {
            if (!Playlist.Contains(e))
            {
                Playlist.Add(e);
                CloudService.Instance.SendSongAdded(e, Playlist.IndexOf(e));
            }

            Debug.WriteLine("Song Requested " + e.Title);
        }

        private void Instance_SongAdded(object sender, SongAddedEventArgs e)
        {
            Playlist.Insert(e.Index, e.Song);
            Debug.WriteLine("Song Added " + e.Song.Title);
        }

        private void Instance_SomeoneDisconnected(object sender, string e)
        {
            var user = Users.FirstOrDefault(u => u.TwitterId == e);
            if (user != null)
                Users.Remove(user);

            Debug.WriteLine("Someone disconnected " + e);
        }

        private void Instance_SomeoneConnected(object sender, User user)
        {
            if (!Users.Contains(user))
                Users.Add(user);
        }

        private void Instance_PartyEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("Party ended");
        }

        public void RaisePropertyChanged([CallerMemberName] String propertyName = null)
        {
            if (Dispatcher == null)
                return;

            Dispatcher.BeginInvoke(() =>
                   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
