using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.PCL.Models
{
    public class PartyJoinResult
    {
        public bool Success { get; set; }
        public List<Song> Songs { get; set; }
        public PartyStatus Status { get; set; }
        public List<User> Users { get; set; }
    }
}
