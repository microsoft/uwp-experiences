using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;

namespace Adventure_Works
{
    public class AdventureRemoteSystem : IDisposable
    {

        public RemoteSystem RemoteSystem { get; private set; }
        private AppServiceConnection _appService;

        public event EventHandler<AdventureRemoteSystemMessageReceivedEventArgs> MessageReceived;

        private AdventureRemoteSystem(RemoteSystem system, AppServiceConnection connection)
        {
            _appService = connection;
            RemoteSystem = system;

            _appService.RequestReceived += AppService_RequestReceived;
            _appService.ServiceClosed += AppService_ServiceClosed;
        }

        public async static Task<AdventureRemoteSystem> CreateAdventureRemoteSystem(RemoteSystem system)
        {
            if (system == null)
            {
                return null;
            }

            var appService = new AppServiceConnection()
            {
                AppServiceName = "com.adventure",
                PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName
            };

            RemoteSystemConnectionRequest connectionRequest = new RemoteSystemConnectionRequest(system);
            var status = await appService.OpenRemoteAsync(connectionRequest);

            if (status != AppServiceConnectionStatus.Success)
            {
                return null;
            }

            var message = new ValueSet();
            message.Add("ping", "");
            var response = await appService.SendMessageAsync(message).AsTask().ConfigureAwait(false);

            if (response.Status != AppServiceResponseStatus.Success)
            {
                return null;
            }

            return new AdventureRemoteSystem(system, appService);
        }

        private async Task<bool> InitializeAppService()
        {
            try
            {
                _appService = new AppServiceConnection()
                {
                    AppServiceName = "com.adventure",
                    PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName
                };
                RemoteSystemConnectionRequest connectionRequest = new RemoteSystemConnectionRequest(RemoteSystem);
                var status = await _appService.OpenRemoteAsync(connectionRequest).AsTask().ConfigureAwait(false);

                if (status == AppServiceConnectionStatus.Success)
                {
                    _appService.RequestReceived += AppService_RequestReceived;
                    _appService.ServiceClosed += AppService_ServiceClosed;
                }
                else
                {
                    CleanUpAppService();
                }

                return status == AppServiceConnectionStatus.Success;
            }
            catch (Exception) {}
            
            return false;
        }

        public async Task<ValueSet> SendMessage(ValueSet message)
        {
            if (_appService == null && !await InitializeAppService().ConfigureAwait(false))
            {
                return null;
            }

            try
            {
                var response = await _appService.SendMessageAsync(message);
                return response.Message;
            }
            catch (Exception) {}

            return null;
        }

        private void AppService_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            CleanUpAppService();
        }

        private async void AppService_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var e = new AdventureRemoteSystemMessageReceivedEventArgs()
            {
                Message = args.Request.Message,
                ResponseMessage = new ValueSet()
            };

            MessageReceived?.Invoke(this, e);

            var status = await args.Request.SendResponseAsync(e.ResponseMessage);
        }

        private void CleanUpAppService()
        {
            if (_appService != null)
            {
                _appService.RequestReceived -= AppService_RequestReceived;
                _appService.ServiceClosed -= AppService_ServiceClosed;
                _appService.Dispose();
                _appService = null;
            }
        }

        public void Dispose()
        {
            CleanUpAppService();
        }
    }
}
