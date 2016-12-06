using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace Adventure_Works
{
    public static class Maps
    {
        public async static Task<Geopoint> GetCurrentLocationAsync()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // Get the current location.
                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    return pos.Coordinate.Point;

                default:
                    // Handle the case if  an unspecified error occurs  by providing hardcoded loc.
                    return null;
            }
        }

        public static double GetDistanceBetweenPoints(BasicGeoposition pos1, BasicGeoposition pos2)
        {
            var dlon = (pos2.Longitude - pos1.Longitude) * Math.PI / 180;
            var dlat = (pos2.Latitude - pos1.Latitude) * Math.PI / 180;

            var a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(pos1.Latitude * Math.PI / 180) * Math.Cos(pos2.Latitude * Math.PI / 180) * Math.Pow(Math.Sin(dlon / 2), 2);
            return 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)) * 6371; 
        }

    }
}
