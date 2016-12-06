using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music.PCL
{
    public class MTObservableCollection<T> : ObservableCollection<T>
    {
        public IDispatcher Dispatcher { get; set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Dispatcher != null)
                Dispatcher.BeginInvoke(() =>
                   base.OnCollectionChanged(e));
        }
    }
}
