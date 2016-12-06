using CoreGraphics;
using Foundation;
using Music.PCL.Models;
using System;
using UIKit;

namespace Music.tvOS
{
    public partial class SongCollectionViewCellNew : UICollectionViewCell
    {
        public UIImageView AlbumArtView;
        public UILabel SongName;

        private Song _song;

        public Song Song
        {
            get { return _song; }
            set
            {
                _song = value;
                if (_song == null) return;

                AlbumArtView.Image = FromUrl(value.AlbumArtMedium);
                SongName.Text = value.Title;
            }
        }

        private UIImage FromUrl(string uri)
        {
            using (var url = new NSUrl(uri))
            using (var data = NSData.FromUrl(url))
                return UIImage.LoadFromData(data);
        }


        public SongCollectionViewCellNew(IntPtr handle) : base (handle)
        {
            AlbumArtView = new UIImageView(new CGRect(35, 0, 480, 480));
            AlbumArtView.AdjustsImageWhenAncestorFocused = true;
            AddSubview(AlbumArtView);

            SongName = new UILabel(new CGRect(35, 490, 480, 50))
            {
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.White,
                Alpha = 0f
            };

            AddSubview(SongName);
        }
    }
}