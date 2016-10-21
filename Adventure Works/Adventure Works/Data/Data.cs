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

namespace Adventure_Works
{
    public class Data
    {
        private const string _filename = "data.aw";

        private AdventureObjectStorageHelper _localStorageService;

        private List<PhotoData> _photos;

        private static Data _instance;

        public static Data Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Data();
                }

                return _instance;
            }
        }

        public Data()
        {
            _localStorageService = new AdventureObjectStorageHelper();
        }

        public async Task<List<PhotoData>> GetPhotosAsync()
        {
            if (_photos == null && !await _localStorageService.FileExistsAsync(_filename))
            {
                _photos = new List<PhotoData>();
                await _localStorageService.SaveFileAsync(_filename, _photos);
            }
            else if (_photos == null)
            {
                _photos = await _localStorageService.ReadFileAsync<List<PhotoData>>(_filename);
            }

            return _photos;
        }

        public async Task<PhotoData> SavePhotoAsync(PhotoData photo)
        {
            var photos = await GetPhotosAsync();
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

            await _localStorageService.SaveFileAsync(_filename, photos);
            _photos = photos;

            return photo;
        }

        public async Task SavePhotosAsync(IEnumerable<PhotoData> newPhotos)
        {
            var photos = await GetPhotosAsync();

            foreach (var photo in newPhotos)
            {
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
            }

            await _localStorageService.SaveFileAsync(_filename, photos);
            _photos = photos;
        }
    }

    public class PhotoData
    {
        public Guid Id { get; set; }
        public IEnumerable<PhotoFace> People { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public DateTime DateTime { get; set; }
        public string Uri { get; set; }
        public string ThumbnailUri { get; set; }
        public bool IsProcessedForFaces { get; set; } = false;
        public bool IsAnalyzed{ get; set; } = false;
    }
}
