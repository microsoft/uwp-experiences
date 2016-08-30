using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Presidents
{
    public sealed class ImageConverter : IValueConverter
    {
        public ImageConverter() { }

        public object Convert(object value, Type targetType,
                              object parameter, string culture)
        {
            if (value is string)
                return new BitmapImage(new Uri((string)value));
            else if (value is Uri)
            {
                return new BitmapImage((Uri)value);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
