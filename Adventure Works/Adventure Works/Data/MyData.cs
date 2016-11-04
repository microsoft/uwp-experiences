using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure_Works.Data
{
    public class MyData
    {
        public List<Adventure> MyAdventures { get; set; }
        public List<Adventure> FriendsAdventures { get; set; }
        public Adventure CurrentAdventure { get; set; }
    }
}
