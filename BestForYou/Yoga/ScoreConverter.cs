using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Yoga
{
    public class ScoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double score = double.Parse(value.ToString());

            if (score < 20)
            {
                return new SolidColorBrush() { Color = Colors.Red };
            }

            if (score < 75)
            {
                return new SolidColorBrush() { Color = Colors.Orange };
            }

            return new SolidColorBrush() { Color = Colors.MediumSeaGreen };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
