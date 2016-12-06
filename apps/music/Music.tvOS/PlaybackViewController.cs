using AVFoundation;
using Foundation;
using Music.PCL;
using Music.PCL.Models;
using Music.PCL.Services;
using Music.SC;
using Music.SC.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UIKit;

namespace Music.tvOS
{
    public partial class PlaybackViewController : UIViewController
    {
        AVPlayer player;

        NSTimer timer;
        Song currentSong;

        public PlaybackViewController (IntPtr handle) : base (handle)
        {
            timer = NSTimer.CreateRepeatingScheduledTimer(1, OnTimer);
        }

        private void OnTimer(NSTimer obj)
        {
            if (player != null && currentSong != null)
            {
                if (player.Rate == 0 && player.CurrentTime.Seconds > currentSong.Duration - 2)
                {
                    NextSong();
                }

                InvokeOnMainThread(() =>
               {
                   var progress = (float)player.CurrentTime.Seconds / (float)currentSong.Duration;
                   SongProgressView.Progress = progress;
                   CloudService.Instance.SendStatusUpdate(GetPartyStatus());
               });
            }
        }

        public override UIView PreferredFocusedView
        {
            get
            {
                return SongsView;
            }
        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            PlayButton.SetBackgroundImage(UIImage.FromFile("images/play.png"), UIControlState.Normal);
            PlayButton.SetBackgroundImage(UIImage.FromFile("images/play_black.png"), UIControlState.Focused);

            PreviousButton.SetBackgroundImage(UIImage.FromFile("images/previous.png"), UIControlState.Normal);
            PreviousButton.SetBackgroundImage(UIImage.FromFile("images/previous_black.png"), UIControlState.Focused);

            NextButton.SetBackgroundImage(UIImage.FromFile("images/next.png"), UIControlState.Normal);
            NextButton.SetBackgroundImage(UIImage.FromFile("images/next_black.png"), UIControlState.Focused);

            PlayButton.AdjustsImageWhenHighlighted = true;
            PreviousButton.AdjustsImageWhenHighlighted = true;
            NextButton.AdjustsImageWhenHighlighted = true;

            PlayButton.AdjustsImageWhenDisabled = true;
            PreviousButton.AdjustsImageWhenDisabled = true;
            NextButton.AdjustsImageWhenDisabled = true;

            PlayButton.Enabled = false;
            PreviousButton.Enabled = false;
            NextButton.Enabled = false;

            PlayButton.PrimaryActionTriggered += PlayButton_PrimaryActionTriggered;
            PreviousButton.PrimaryActionTriggered += PreviousButton_PrimaryActionTriggered;
            NextButton.PrimaryActionTriggered += NextButton_PrimaryActionTriggered;

            UIVisualEffect blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
            UIVisualEffectView visualEffectView = new UIVisualEffectView(blurEffect);
            visualEffectView.Frame = new CoreGraphics.CGRect(0, -100, 1920, 1200);
            BackgroundImage.AddSubview(visualEffectView);

            CloudService.Instance.Init(new User()
            {
                TwitterId = "fakeId",
                Name = "Fake user"
            });
            CloudService.Instance.Connected += Instance_Connected;

            DataService.Instance.Dispatcher = new DispatcherWrapper(this);

        }

        private void UpdateButtonViews()
        {
            if (player != null)
            {
                if (player.Rate == 0)
                {
                    PlayButton.SetBackgroundImage(UIImage.FromFile("images/play.png"), UIControlState.Normal);
                    PlayButton.SetBackgroundImage(UIImage.FromFile("images/play_black.png"), UIControlState.Focused);
                }
                else
                {
                    PlayButton.SetBackgroundImage(UIImage.FromFile("images/pause.png"), UIControlState.Normal);
                    PlayButton.SetBackgroundImage(UIImage.FromFile("images/pause_black.png"), UIControlState.Focused);
                }

                PlayButton.Enabled = true;
            }
            else
            {
                PlayButton.SetBackgroundImage(UIImage.FromFile("images/play.png"), UIControlState.Normal);
                PlayButton.SetBackgroundImage(UIImage.FromFile("images/play_black.png"), UIControlState.Focused);
                PlayButton.Enabled = false;
            }

            if (DataService.Instance.Playlist != null && DataService.Instance.Playlist.Count > 0 && currentSong != null)
            {
                var index = DataService.Instance.Playlist.IndexOf(currentSong);

                if (index > -1)
                {
                    NextButton.Enabled = index < DataService.Instance.Playlist.Count - 1;
                    PreviousButton.Enabled = index > 0;
                    return;
                }
            }

            NextButton.Enabled = false;
            PreviousButton.Enabled = false;

            
        }

        private void NextButton_PrimaryActionTriggered(object sender, EventArgs e)
        {
            NextSong();
        }

        private void NextSong()
        {
            if (player != null)
            {
                var index = DataService.Instance.Playlist.IndexOf(currentSong);
                PlaySong(DataService.Instance.Playlist[index + 1]);
            }
        }

        private void PreviousButton_PrimaryActionTriggered(object sender, EventArgs e)
        {
            PreviousSong();
        }

        private void PreviousSong()
        {
            if (player != null)
            {
                var index = DataService.Instance.Playlist.IndexOf(currentSong);
                PlaySong(DataService.Instance.Playlist[index - 1]);
            }
        }

        private void PlayButton_PrimaryActionTriggered(object sender, EventArgs e)
        {
            if (player != null)
            {
                if (player.Rate == 0)
                    player.Play();
                else
                    player.Pause();
            }

            UpdateButtonViews();
        }

        private void Instance_Connected(object sender, EventArgs e)
        {
            InvokeOnMainThread(async () =>
            {
                await DataService.Instance.InitDataServiceAsHost();
                DataService.Instance.Playlist.CollectionChanged += Playlist_CollectionChanged;

                var source = new SongViewDataSource(DataService.Instance.Playlist);
                SongsView.SongFocused += SongsView_SongFocused;

                SongsView.DataSource = source;
                SongsView.ReloadData();

                View.SetNeedsFocusUpdate();
                View.UpdateFocusIfNeeded();

                PartyCodeLabel.Text = "The code is " + DataService.Instance.PartyCode;
            });
        }

        private void Playlist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SongsView.ReloadData();
        }

        private PartyStatus GetPartyStatus()
        {
            var status = new PartyStatus();

            if (player != null && currentSong != null)
            {
                status.Duration = currentSong.Duration;
                status.TrackIndex = DataService.Instance.Playlist.IndexOf(currentSong);
                status.State = player.Rate != 0 ? PlaybackState.Playing : PlaybackState.Paused;
                status.Progress = (int)player.CurrentTime.Seconds;
            }
            else
            {
                status.State = PlaybackState.Other;
            }

            return status;
        }

        private void SongsView_SongFocused(object sender, Song e)
        {
            if (currentSong != e)
            {
                PlaySong(e);
            }
        }

        private void PlaySong(Song song)
        {
            currentSong = song;
            SongNameLabel.Text = song.Title;
            ArtistName.Text = song.Artist;
            BackgroundImage.Image = FromUrl(song.AlbumArtLarge);
            BackgroundImage.Alpha = 0.6f;
            BackgroundImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            Play(song.StreamUrl);

            SongsView.CenterSong(currentSong);

            UpdateButtonViews();
        }

        private UIImage FromUrl(string uri)
        {
            using (var url = new NSUrl(uri))
            using (var data = NSData.FromUrl(url))
                return UIImage.LoadFromData(data);
        }

        private void Play(string url)
        {
            if (player != null)
            {
                player.Dispose();
                player = null;
            }
            
            player = AVPlayer.FromUrl(new NSUrl(url));

            player.Play();
        }
    }
}