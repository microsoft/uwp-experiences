using Newtonsoft.Json;
using Music.SC.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using Music.PCL.Models;
using Music.PCL;

namespace Music.SC
{
    public class API
    {
        private string BASEURL = "https://api.soundcloud.com";

        private string clientId;
        private string clientSecret;
        private string redirectUri;

        private static API _instance;

        public static API Instance { get
            {
                if (_instance == null)
                {
                    _instance = new API(Keys.SC_CLIENT_ID, Keys.SC_CLIENT_SECRET, Keys.SC_REDIRECT_URL);
                }
                return _instance;
            }
        }

        public static void Init(string clientId, string clientSecret, string redirectUri)
        {
            _instance = new SC.API(clientId, clientSecret, redirectUri);
        }

        public API(string clientId, string clientSecret, string redirectUri)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUri = redirectUri;
        }

        private async Task<string> callAPI(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(new Uri(url));
                HttpResponseMessage v = new HttpResponseMessage();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<List<Track>> GetTracksForUser(string user)
        {
            string url = $"http://api.soundcloud.com/users/{user}/tracks?client_id={clientId}";
            var responseString = await this.callAPI(url);

            try
            {
                List<Track> tracks = JsonConvert.DeserializeObject<List<Track>>(responseString);
                return tracks;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<List<Track>> GetPlaylistTracks(string playlistId)
        {
            string url = $"http://api.soundcloud.com/playlists/{playlistId}?client_id={clientId}";
            var responseString = await this.callAPI(url);

            try
            {
                List<Track> tracks = JsonConvert.DeserializeObject<Playlist>(responseString).tracks;
                return tracks;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public Uri getStreamUrl(Track track)
        {
            return new Uri(track.stream_url + "?client_id=" + this.clientId);
        }

        public string getStreamUrl(string url)
        {
            return url + "?client_id=" + this.clientId;
        }

        public async Task<SCUser> GetUser(string userID)
        {
            var url = BASEURL + "/users/" + userID + ".json?client_id=" + this.clientId;
            var responseString = await this.callAPI(url);

            SCUser user = JsonConvert.DeserializeObject<SCUser>(responseString);
            return user;
        }

        public static Song GetSong(Track track)
        {
            return new Song()
            {
                ItemId = track.id.ToString(),
                Artist = track.user.username,
                Title = track.title,
                AlbumArt = track.artwork_url,
                AlbumArtMedium = track.artwork_url.Replace("large", "t300x300"),
                AlbumArtLarge = track.artwork_url.Replace("large", "t500x500"),
                Duration = track.duration / 1000,
                StreamUrl = SC.API.Instance.getStreamUrl(track).ToString()
            };
        }

    }
}
