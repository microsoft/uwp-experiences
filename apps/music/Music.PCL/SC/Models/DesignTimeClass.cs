using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.SC.Models
{
    public class DesignTimeClass
    {
        private static DesignTimeClass _dataSource = new DesignTimeClass();

        public static DesignTimeClass AppDataSource
        {
            get { return _dataSource; }
        }

        

        public DesignTimeClass()
        {
            Track track = new Track();

            track.title = "temp";
            
        }
    }
}
