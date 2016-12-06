using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using UIKit;
using Music.PCL.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Music.tvOS
{
    public class SongViewDataSource : UICollectionViewDataSource
    {
        public static AppDelegate App
        {
            get { return (AppDelegate)UIApplication.SharedApplication.Delegate; }
        }

        public static NSString CardCellId = new NSString("SongCell");

        public ObservableCollection<Song> Songs { get; set; }

        public Dictionary<Song, SongCollectionViewCellNew> Cells = new Dictionary<Song, SongCollectionViewCellNew>();

        public SongViewDataSource( ObservableCollection<Song> songs)
        {
            Songs = songs;
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var songCell = (SongCollectionViewCellNew)collectionView.DequeueReusableCell(CardCellId, indexPath);
            var song = Songs[indexPath.Row];
            
            songCell.Song= song;

            Cells[song] = songCell;

            return songCell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return Songs.Count;
        }
    }
}
