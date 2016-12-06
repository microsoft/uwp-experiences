using Adventure_Works.CognitiveServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.Devices.Geolocation;
using System.Threading;

namespace Adventure_Works.Data
{
    public class DataProvider
    {
        private const string _filename = "data.json";
        private AdventureObjectStorageHelper _localStorageService;
        private MyData _data;
        private static DataProvider _instance;

        private ManualResetEventSlim _mResetEvent;
        private bool _loadingDummyData = false;

        public static DataProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataProvider();
                }

                return _instance;
            }
        }

        public DataProvider()
        {
            _localStorageService = new AdventureObjectStorageHelper();
            _mResetEvent = new ManualResetEventSlim(true);
        }

        private async Task<MyData> GetData()
        {
            if (_data == null && !await _localStorageService.FileExistsAsync(_filename))
            {
                if (_loadingDummyData)
                {
                    await Task.Run(() => _mResetEvent.Wait());
                    return await GetData();
                }

                _loadingDummyData = true;
                _mResetEvent.Reset();

                _data = await LoadDummyData();
                await _localStorageService.SaveFileAsync(_filename, _data);

                _mResetEvent.Set();
                _loadingDummyData = false;

                return _data;
            }

            return _data ?? (_data = await _localStorageService.ReadFileAsync<MyData>(_filename));
        }

        private async Task<MyData> LoadDummyData()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/data.json"));
            await file.CopyAsync(await AdventureObjectStorageHelper.GetDataSaveFolder(), _filename, NameCollisionOption.ReplaceExisting);
            var data = await _localStorageService.ReadFileAsync<MyData>(_filename);

            return data;
        }

        public async Task<Adventure> GetLatestAdventureByUser(User user)
        {
            var data = await GetData();
            return data.FriendsAdventures.Where(a => a.User == user).FirstOrDefault();
        }

        public async Task<List<Adventure>> GetMyAdventures()
        {
            var data = await GetData();
            return data.MyAdventures;
        }

        public async Task<List<Adventure>> GetFriendsAdventures()
        {
            var data = await GetData();
            return data.FriendsAdventures;
        }

        public async Task<Adventure> GetCurrentAdventure()
        {
            var data = await GetData();
            return data.CurrentAdventure;
        }

        public async Task<Adventure> GetAdventure(string id)
        {
            var data = await GetData();

            if (data.CurrentAdventure != null && id == data.CurrentAdventure.Id.ToString())
                return data.CurrentAdventure;

            return data.MyAdventures.Concat(data.FriendsAdventures).Where(a => a.Id.ToString() == id).FirstOrDefault();
        }

        public async Task ClearSavedFaces()
        {
            var data = await GetData();
            foreach (var photo in data.CurrentAdventure.Photos)
            {
                photo.IsProcessedForFaces = false;
            }

            await _localStorageService.SaveFileAsync(_filename, data);
        }

        public async Task<PhotoData> GetPhotoAsync(string id)
        {
            var data = await GetData();

            return data.FriendsAdventures.Concat(data.MyAdventures).Concat(new Adventure[] { data.CurrentAdventure }).SelectMany(a => a.Photos).Where(p => p.Id.ToString() == id).FirstOrDefault();
        }

        public async Task<PhotoData> SavePhotoAsync(PhotoData photo)
        {
            var data = await GetData();
            
            if (photo.Id == default(Guid))
            {
                photo.Id = Guid.NewGuid();
                data.CurrentAdventure.Photos.Add(photo);
            }

            await _localStorageService.SaveFileAsync(_filename, _data);

            return photo;
        }

        public async Task<Adventure> CreateNewAdventure(string name)
        {
            var data = await GetData();
            if (data.CurrentAdventure != null)
                return data.CurrentAdventure;

            var adventure = new Adventure()
            {
                Id = Guid.NewGuid(),
                InProgress = true,
                Location = (await Maps.GetCurrentLocationAsync()).Position,
                Name = name,
                People = new List<User>(),
                Photos = new List<PhotoData>(),
            };

            data.CurrentAdventure = adventure;
            await _localStorageService.SaveFileAsync(_filename, _data);

            return adventure;
        }

        public async Task<User> GetUserByName(string name)
        {
            var data = await GetData();
            var users = data.FriendsAdventures.Select(a => a.User);

            var user = users.Where(u => u.Name == name).FirstOrDefault();

            if (user != null)
                return user;

            int minDistance = int.MaxValue;

            foreach (var someUser in users)
            {
                var distance = LevenshteinDistance(name, someUser.Name);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    user = someUser;
                }
            }

            return user;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

    }
}
