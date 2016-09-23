using Music.PCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace music_appService.DataObjects
{
    public class Party
    {
        public string OwnerId { get; set; }
        public string OwnerTwitterId { get; set; }
        public string Code { get; set; }
        public List<User> Users { get; set; } = new List<User>();
        public List<Song> Songs { get; set; }
        public PartyStatus LatestStatus { get; set; }
    }
}