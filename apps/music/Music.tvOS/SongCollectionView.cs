using Foundation;
using Music.PCL.Models;
using System;
using UIKit;

namespace Music.tvOS
{
    public partial class SongCollectionView : UICollectionView
    {
        public SongCollectionView (IntPtr handle) : base (handle)
        {
            RegisterClassForCell(typeof(SongCollectionViewCellNew), SongViewDataSource.CardCellId);

            (this.CollectionViewLayout as UICollectionViewFlowLayout).SectionInset = new UIEdgeInsets(0, 715, 0, 715);
        }

        public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
        {
            var nextItem = context.NextFocusedView as SongCollectionViewCellNew;
            if (nextItem != null)
            {
                SongFocused?.Invoke(this, nextItem.Song);
            }
        }

        public void CenterSong(Song song)
        {
            var datasource = (this.DataSource as SongViewDataSource);
            var cell = datasource.Cells[song];
            if (cell != null)
            {
                var path = this.IndexPathForCell(cell);
                ScrollToItem(path, UICollectionViewScrollPosition.CenteredHorizontally, true);
            }
        }

        public event EventHandler<Song> SongFocused;
    }
}