using Music.Helpers;
using Music.PCL;
using Music.PCL.Models;
using Music.PCL.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Music
{
    public sealed partial class SongList : UserControl
    {
        public SongList()
        {
            this.InitializeComponent();
        }
        
        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(SongList), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var songList = d as SongList;
            songList.items.ItemsSource = e.NewValue;
        }

        public ListType ListType
        {
            get { return (ListType)GetValue(ListTypeProperty); }
            set { SetValue(ListTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListTypeProperty =
            DependencyProperty.Register("ListType", typeof(ListType), typeof(SongList), new PropertyMetadata(ListType.AddSongs, OnListTypeChanged));

        private static void OnListTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var songList = d as SongList;
            switch ((ListType)e.NewValue)
            {
                case ListType.AddSongs:
                    songList.items.ItemTemplate = songList.Resources["AddSongTemplate"] as DataTemplate;
                    break;
                case ListType.LikeSongs:
                    songList.items.ItemTemplate = songList.Resources["LikeSongTemplate"] as DataTemplate;
                    break;
            }
        }

        private ObservableCollection<Song> Songs = new ObservableCollection<Song>();

        public event EventHandler<Song> AddClicked;
        public event EventHandler<SongLikedEventArgs> LikeChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var song = (sender as Button).DataContext as Song;
            AddClicked?.Invoke(this, song);
        }

        private void LikeChecked(object sender, RoutedEventArgs e)
        {
            var song = (sender as RadioButton).DataContext as Song;
            LikeChanged?.Invoke(this, new SongLikedEventArgs() { Song = song, State = LikeState.Liked });
            
        }

        private void LikeUnchecked(object sender, RoutedEventArgs e)
        {
            var song = (sender as RadioButton).DataContext as Song;
            LikeChanged?.Invoke(this, new SongLikedEventArgs() { Song = song, State = LikeState.UnLiked });
        }

        private void DislikeChecked(object sender, RoutedEventArgs e)
        {
            var song = (sender as RadioButton).DataContext as Song;
            LikeChanged?.Invoke(this, new SongLikedEventArgs() { Song = song, State = LikeState.Disliked });
            
        }

        private void DislikeUnchecked(object sender, RoutedEventArgs e)
        {
            var song = (sender as RadioButton).DataContext as Song;
            LikeChanged?.Invoke(this, new SongLikedEventArgs() { Song = song, State = LikeState.UnDisliked });
        }
    }

    public class SongLikedEventArgs
    {
        public Song Song { get; set; }
        public LikeState State { get; set; }
    }

    public enum LikeState
    {
        Liked,
        UnLiked,
        Disliked,
        UnDisliked
    }

    public enum ListType
    {
        AddSongs,
        LikeSongs,
    }
}
