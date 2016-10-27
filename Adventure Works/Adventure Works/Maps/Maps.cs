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
    }
}
