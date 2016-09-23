using Music.PCL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Music.PCL.Models;
using System.Collections.ObjectModel;

namespace Music.PCL
{
    public class ClientViewModel : MainViewModel
    {

        private ObservableCollection<Song> _userSongs = new ObservableCollection<Song>();

        public ObservableCollection<Song> UserSongs
        {
            get { return _userSongs; }
            set
            {
                _userSongs = value;
                RaisePropertyChanged();
            }
        }


        public ClientViewModel()
        {
            SongCollection = DataService.Instance.Playlist;
            CloudService.Instance.PartyStateUpdated += Instance_PartyStateUpdated;
        }

        private void Instance_PartyStateUpdated(object sender, PartyStatus e)
        {
            if (e != null)
            {
                CurrentItem = SongCollection.ElementAt(e.TrackIndex);
                CurrentItemIndex = e.TrackIndex;
                Duration = e.Duration;
                Position = e.Progress;
                PlayerState = e.State;
            }
        }

        public void AddSong(Song song)
        {
            CloudService.Instance.RequestSongAdd(song);
        }

        public async Task LoadUserSongs()
        {
            var tracks = await SC.API.Instance.GetTracksForUser(Keys.SC_USER_ID);

            foreach (var track in tracks)
            {
                UserSongs.Add(SC.API.GetSong(track));
            }
        }
    }
}
