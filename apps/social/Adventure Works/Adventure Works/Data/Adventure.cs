using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Adventure_Works.Data
{
    public class Adventure
    {
        public Guid Id { get; set; }

        public User User { get; set; }

        public List<User> People { get; set; }

        public BasicGeoposition Location { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<PhotoData> Photos { get; set; }

        public bool InProgress { get; set; }
    }
}
