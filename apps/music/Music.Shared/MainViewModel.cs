using Music.PCL.Models;
using Music.PCL.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Music.PCL
{
    public abstract class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IDispatcher Dispatcher;

        private ObservableCollection<Song> _songCollection;

        public virtual ObservableCollection<Song> SongCollection
        {
            get { return _songCollection; }
            set
            {
                _songCollection = value;
                RaisePropertyChanged();
            }
        }

        private int _currentItemIndex;

        public virtual int CurrentItemIndex
        {
            get { return _currentItemIndex; }
            set
            {
                _currentItemIndex = value;
                RaisePropertyChanged();
            }
        }

        private Song _currentItem;

        public virtual Song CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                RaisePropertyChanged();
            }
        }


        private int _position = 0;

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                RaisePropertyChanged();
            }
        }

        private int _duration = 0;

        public int Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                RaisePropertyChanged();
            }
        }

        private PlaybackState _playerState = PlaybackState.Other;

        public PlaybackState PlayerState
        {
            get { return _playerState; }
            set
            {
                _playerState = value;
                RaisePropertyChanged();
            }
        }

        private bool _playButtonEnabled;

        public bool PlayButtonEnabled
        {
            get { return _playButtonEnabled; }
            set {
                _playButtonEnabled = value;
                RaisePropertyChanged();
            }
        }

        public void RaisePropertyChanged([CallerMemberName] String propertyName = null)
        {
            if (Dispatcher == null)
                return;

            Dispatcher.BeginInvoke( () =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
