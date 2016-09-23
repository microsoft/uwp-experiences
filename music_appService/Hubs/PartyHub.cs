using Microsoft.AspNet.SignalR;
using music_appService.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Music.PCL.Models;
using System.Net.Http;

namespace music_appService.Hubs
{
    public class PartyHub : Hub
    {
        private static List<Party> _parties { get; set; } = new List<Party>();

        public async Task<string> StartParty(User user, List<Song> songs, PartyStatus status)
        {
            var twitterId = Context.Headers["twitterId"];

            var existingParty = _parties.FirstOrDefault(k => k.OwnerId == twitterId);

            if (existingParty != null)
            {
                existingParty.Songs = songs;
                existingParty.LatestStatus = status;
                existingParty.Users.RemoveAll(u => u.TwitterId == twitterId);

                user.ConnectionId = Context.ConnectionId;
                existingParty.Users.Add(user);

                return existingParty.Code;
            }

            var party = new Party()
            {
                OwnerId = twitterId,
                Code = await GetCode(),
                Songs = songs,
                LatestStatus = status
            };

            user.ConnectionId = Context.ConnectionId;
            party.Users.Add(user);
            _parties.Add(party);
            return party.Code;
        }


        public PartyJoinResult JoinParty(User user, string code)
        {
            var result = new PartyJoinResult();
            var party = _parties.FirstOrDefault(p => p.Code.ToLower() == code.ToLower());

            if (party == null)
            {
                result.Success = false;
                return result;
            }

            result.Success = true;
            result.Songs = party.Songs;
            result.Users = party.Users;
            result.Status = party.LatestStatus;

            var existingUser = party.Users.FirstOrDefault(u => u.TwitterId == user.TwitterId);

            if (existingUser != null)
            {
                existingUser.ConnectionId = Context.ConnectionId;
            }
            else
            {
                user.ConnectionId = Context.ConnectionId;
                party.Users.Add(user);
            }
            
            Clients.Clients(party.Users.Where(u => u.TwitterId != user.TwitterId).Select(u => u.ConnectionId).ToList()).UserAddedToParty(user);

            return result;
        }

        public void StatusUpdate(PartyStatus status)
        {
            var twitterId = Context.Headers["twitterId"];

            var party = _parties.FirstOrDefault(p => p.OwnerId == twitterId);

            if (party == null)
            {
                return;
            }

            party.LatestStatus = status;

            Clients.Clients(party.Users.Where(u => u.TwitterId != twitterId).Select(u => u.ConnectionId).ToList()).StateUpdate(status);
        }

        public void SongAdded(Song song, int index)
        {
            var twitterId = Context.Headers["twitterId"];

            var party = _parties.FirstOrDefault(p => p.OwnerId == twitterId);

            if (party == null)
            {
                return;
            }

            Clients.Clients(party.Users.Where(u => u.TwitterId != twitterId).Select(u => u.ConnectionId).ToList()).SongUpdate(song, index);
        }

        public void AddSong(Song song)
        {
            var twitterId = Context.Headers["twitterId"];

            var party = _parties.Where(p => p.Users.FindIndex(u => u.TwitterId == twitterId) >= 0).FirstOrDefault();

            if (party == null)
            {
                return;
            }

            var connectionId = party.Users.First(u => u.TwitterId == party.OwnerId).ConnectionId;
            Clients.Client(connectionId).UserRequestedSong(song);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var party = _parties.FirstOrDefault(p => p.OwnerId == Context.ConnectionId);

            if (party != null)
            {
                Clients.Clients(party.Users.Select(u => u.ConnectionId).ToList()).PartyOver();
                _parties.Remove(party);
            }

            var parties = _parties.Where(p => p.Users.All(u => u.ConnectionId == Context.ConnectionId));

            foreach (var p in parties)
            {
                Clients.Clients(p.Users.Select(u => u.ConnectionId).ToList()).UserDisconnected(Context.ConnectionId);
            }

            return base.OnDisconnected(stopCalled);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> GetCode()
        {
            string code = "";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    code = await client.GetStringAsync(new Uri("http://randomword.setgetgo.com/get.php?len=6"));
                }
                catch (Exception ex)
                {
                    code = RandomString(4);
                }

            }
            return code.ToLower();
        }
    }
}