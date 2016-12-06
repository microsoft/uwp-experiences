using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Atmosphere
{
    public class WeatherConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var weatherType = value.ToString();

            var msappxUrl = "";

            switch (weatherType)
            {
                case "HeavyRain":
                    msappxUrl = "ms-appx:///Assets/HeavyRain.png";
                    break;
                case "CloudyWithSun":
                    msappxUrl = "ms-appx:///Assets/SunshineAndCloud.png";
                    break;
                case "Sunny":
                    msappxUrl = "ms-appx:///Assets/Sunshine.png";
                    break;
                case "Cloudy":
                    msappxUrl = "ms-appx:///Assets/Cloud.png";
                    break;
                case "Rain":
                    msappxUrl = "ms-appx:///Assets/Rain.png";
                    break;
                case "Thunder":
                    msappxUrl = "ms-appx:///Assets/Thunderstorm.png";
                    break;
            }

            var bitmapImage = new BitmapImage(new Uri(msappxUrl));
            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
