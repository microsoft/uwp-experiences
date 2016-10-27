using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
using Windows.UI.Core;

namespace Adventure_Works
{
    public class RomeHelper : IDisposable
    {
        private RemoteSystemWatcher _remoteSystemWatcher;

        private ObservableCollection<AdventureRemoteSystem> _availableRemoteSystems = new ObservableCollection<AdventureRemoteSystem>();
        private List<RemoteSystem> _remoteSystems = new List<RemoteSystem>();


        public ObservableCollection<AdventureRemoteSystem> AvailableRemoteSystems
        {
            get { return _availableRemoteSystems; }
        }

        public event EventHandler<AdventureRemoteSystem> RemoteSystemAdded;
        public event EventHandler<AdventureRemoteSystem> RemoteSystemRemoved;

        private RemoteSystem _currentSystem;

        public async Task Initialize()
        {
            if (_remoteSystemWatcher != null)
                return;

            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();
            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                _remoteSystemWatcher = RemoteSystem.CreateWatcher(BuildFilters());
                _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;
                _remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;
                _remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;
                _remoteSystemWatcher.Start();
            }
        }

        private List<IRemoteSystemFilter> BuildFilters()
        {
            List<IRemoteSystemFilter> filters = new List<IRemoteSystemFilter>();

            filters.Add(new RemoteSystemDiscoveryTypeFilter(RemoteSystemDiscoveryType.Proximal));

            filters.Add(new RemoteSystemKindFilter(new string[] { RemoteSystemKinds.Desktop, RemoteSystemKinds.Xbox }));

            filters.Add(new RemoteSystemStatusTypeFilter(RemoteSystemStatusType.Any));

            return filters;
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            _remoteSystems.Add(args.RemoteSystem);
            var system = await AdventureRemoteSystem.CreateAdventureRemoteSystem(args.RemoteSystem).ConfigureAwait(false);
            if (system != null)
            {
                var t = Helpers.RunOnCoreDispatcherIfPossible(() =>
                {
                    if (_availableRemoteSystems.Where(s => s.RemoteSystem.Id == system.RemoteSystem.Id).Count() == 0)
                    {
                        _availableRemoteSystems.Add(system);
                        RemoteSystemAdded?.Invoke(this, system);
                    }

                });
            }
        }

        private void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            var remoteSystem = _remoteSystems.Where(s => s.Id == args.RemoteSystemId).FirstOrDefault();
            if (remoteSystem != null)
            {
                _remoteSystems.Remove(remoteSystem);
            }

            var system = _availableRemoteSystems.Where(s => s.RemoteSystem.Id == args.RemoteSystemId).FirstOrDefault();
            if (system != null)
            {
                var t = Helpers.RunOnCoreDispatcherIfPossible(() =>
                {
                    _availableRemoteSystems.Remove(system);
                    RemoteSystemRemoved?.Invoke(this, system);
                });
            }
        }

        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            var remoteSystem = _remoteSystems.Where(s => s.Id == args.RemoteSystem.Id).FirstOrDefault();
            if (remoteSystem != null)
                return;

            var existingSystem = _availableRemoteSystems.Where(s => s.RemoteSystem.Id == args.RemoteSystem.Id).FirstOrDefault();
            if (existingSystem == null)
            {
                var system = await AdventureRemoteSystem.CreateAdventureRemoteSystem(args.RemoteSystem).ConfigureAwait(false);
                if (system != null)
                {
                    var t = Helpers.RunOnCoreDispatcherIfPossible(() =>
                    {
                        if (_availableRemoteSystems.Where(s => s.RemoteSystem.Id == system.RemoteSystem.Id).Count() == 0)
                        {
                            _availableRemoteSystems.Add(system);
                            RemoteSystemAdded?.Invoke(this, system);
                        }
                    });
                }
            }
        }

        public void Dispose()
        {
            if (_remoteSystemWatcher != null)
            {
                foreach (var system in _availableRemoteSystems)
                {
                    system.Dispose();
                }

                _remoteSystemWatcher.RemoteSystemAdded -= RemoteSystemWatcher_RemoteSystemAdded;
                _remoteSystemWatcher.RemoteSystemRemoved -= RemoteSystemWatcher_RemoteSystemRemoved;
                _remoteSystemWatcher.RemoteSystemUpdated -= RemoteSystemWatcher_RemoteSystemUpdated;
                _remoteSystemWatcher.Stop();
                _remoteSystemWatcher = null;
            }
        }
    }
}
