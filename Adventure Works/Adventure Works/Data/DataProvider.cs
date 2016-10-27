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

        public async Task<PhotoData> SavePhotoAsync(PhotoData photo)
        {
            var data = await GetData();

            if (data.CurrentAdventure == null)
            {
                return null;
            }

            var adventure = data.CurrentAdventure;

            if (adventure == null)
            {
                return null;
            }

            var photos = adventure.Photos;

            var existingPhoto = photos.Where(p => p.Id == photo.Id).FirstOrDefault();

            if (existingPhoto != null)
            {
                var index = photos.IndexOf(existingPhoto);
                photos[index] = photo;
            }
            else
            {
                photo.Id = Guid.NewGuid();
                photos.Add(photo);
            }

            await _localStorageService.SaveFileAsync(_filename, _data);

            return photo;
        }

    }
}
