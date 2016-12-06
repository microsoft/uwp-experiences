using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Atmosphere
{
    public static class WeatherService 
    {
        public static IAsyncOperation<string> GetRawWeatherData()
        {
            var storageFileTask = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///weather.json"));

            return storageFileTask.AsTask().ContinueWith((Task<StorageFile> fileTask) =>
            {
                var rawWeatherData = System.IO.File.ReadAllText(fileTask.Result.Path);
                return rawWeatherData;
            }).AsAsyncOperation<string>();
        }

        public static IAsyncOperation<WeatherData> GetWeatherData()
        {
            var rawWeatherDataTask = GetRawWeatherData();
            return rawWeatherDataTask.AsTask().ContinueWith((Task<string> rawWeatherData) =>
            {
                return JsonConvert.DeserializeObject<WeatherData>(rawWeatherData.Result);

            }).AsAsyncOperation<WeatherData>();
        }
    }
}
