using Microsoft.AspNet.SignalR.Client;
using Music.PCL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.PCL
{
    public class CloudService
    {

        private HubConnection _connection { get; set; }
        private IHubProxy _proxy { get; set; }

        private static CloudService _instance;

        public User User { get; private set; }

        public ServiceState State { get; private set; } = ServiceState.NotInitialized;
        public ParticipationType ParticipationType { get; private set; } = ParticipationType.None;

        public static CloudService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CloudService();
                }

                return _instance;
            }
        }

        // host
        public event EventHandler<Song> SongRequested;

        // both
        public event EventHandler<string> SomeoneDisconnected;
        public event EventHandler<User> SomeoneConnected;

        // client
        public event EventHandler<SongAddedEventArgs> SongAdded;
        public event EventHandler PartyEnded;
        public event EventHandler<PartyStatus> PartyStateUpdated;

        public event EventHandler Connected;

        private CloudService()
        {
            _connection = new HubConnection(Keys.SERVICE_URL);
            _connection.StateChanged += Conn_StateChanged;
            _connection.Error += Conn_Error; ;

            _proxy = _connection.CreateHubProxy("PartyHub");

            // Party owner
            _proxy.On<Song>("UserRequestedSong", (song) => {
                SongRequested?.Invoke(this, song);
            });

            // both
            _proxy.On<string>("UserDisconnected", (userId) => { SomeoneDisconnected?.Invoke(this, userId); });
            _proxy.On<User>("UserAddedToParty", (user) => { SomeoneConnected?.Invoke(this, user); });

            // party user
            _proxy.On("PartyOver", () => { PartyEnded?.Invoke(this, null); });
            _proxy.On<PartyStatus>("StateUpdate", (status) => { PartyStateUpdated?.Invoke(this, status); });
            _proxy.On<Song, int>("SongUpdate", (song, index) =>
            {
                SongAdded?.Invoke(this, new SongAddedEventArgs()
                {
                    Song = song,
                    Index = index
                });
            });
        }

        public bool Init(User currentUser)
        {
            if (string.IsNullOrEmpty(currentUser.TwitterId))
            {
                return false;
            }

            User = currentUser;
            _connection.Headers.Add("twitterId", currentUser.TwitterId);

            State = ServiceState.Disconnected;

            _connection.Start();

            return true;
        }

        private void Conn_Error(Exception obj)
        {
            Debug.WriteLine(obj.Message);
            if (State == ServiceState.NotInitialized)
                return;

            State = ServiceState.Disconnected;
            _connection.Start();
        }

        private void Conn_StateChanged(StateChange obj)
        {
            Debug.WriteLine(obj.NewState);
            if (obj.NewState == ConnectionState.Connected)
            {
                Connected?.Invoke(this, null);
                State = ServiceState.Connected;
            }
            else
            {
                State = ServiceState.Disconnected;
                if (obj.NewState == ConnectionState.Disconnected)
                {
                    _connection.Start();
                }
            }
        }

        // host
        public Task<string> StartParty(List<Song> songs, PartyStatus status)
        {
            if (State == ServiceState.NotInitialized)
                return null;

            ParticipationType = ParticipationType.Host;
            return _proxy.Invoke<string>("StartParty", User, songs, status);
        }

        public void SendStatusUpdate(PartyStatus status)
        {
            if (State == ServiceState.NotInitialized ||
                ParticipationType != ParticipationType.Host)
                return;

            _proxy.Invoke("StatusUpdate", status);
        }

        public void SendSongAdded(Song song, int index)
        {
            if (State == ServiceState.NotInitialized ||
                ParticipationType != ParticipationType.Host)
                return;

            _proxy.Invoke("SongAdded", song, index);
        }


        //client
        public Task<PartyJoinResult> JoinParty(string code)
        {
            if (State == ServiceState.NotInitialized)
                return null;

            ParticipationType = ParticipationType.Participant;
            return _proxy.Invoke<PartyJoinResult>("JoinParty", User, code);
        }

        public void RequestSongAdd(Song song)
        {
            if (State == ServiceState.NotInitialized ||
                ParticipationType != ParticipationType.Participant)
                return;

            _proxy.Invoke<bool>("AddSong", song);
        }
    }

    public class SongAddedEventArgs
    {
        public Song Song { get; set; }
        public int Index{ get; set; }
    }

    public enum ParticipationType
    {
        None,
        Host,
        Participant
    }

    public enum ServiceState
    {
        Connected,
        Disconnected,
        NotInitialized
    }
}
