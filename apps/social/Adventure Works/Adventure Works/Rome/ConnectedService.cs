using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.RemoteSystems;

namespace Adventure_Works.Rome
{
    public class ConnectedService : IDisposable
    {
        private AppServiceConnection _appServiceConnection;
        private AdventureRemoteSystem _remoteSystem;

        private ConnectedServiceStatus _previousStatus;
        private ConnectedServiceStatus _status;
        private static ConnectedService _instance;

        public event EventHandler<SlideshowMessageReceivedEventArgs> ReceivedMessageFromClient;
        public event EventHandler<SlideshowMessageReceivedEventArgs> ReceivedMessageFromHost;
        public event EventHandler HostingConnectionStoped;

        public ConnectedServiceStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public static ConnectedService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConnectedService();
                }
                return _instance;
            }
        }

        public RomeHelper Rome { get; private set; }

        private ConnectedService()
        {
            // doesn't make sense to discover devices from Xbox in this case
            if (!App.IsXbox())
            {
                Rome = new RomeHelper();
                Rome.Initialize();
            }
        }

        /// <summary>
        /// Handles any received messages and routes them to the correct handler
        /// </summary>
        /// <param name="message"></param>
        /// <param name="returnMessage"></param>
        /// <param name="appServiceConnection"> used when message is to start a connection</param>
        /// <returns></returns>
        public async Task<ValueSet> HandleMessageReceivedAsync(ValueSet message, ValueSet returnMessage, AppServiceConnection appServiceConnection = null)
        {
            if (returnMessage == null)
                returnMessage = new ValueSet();

            if (message.ContainsKey("query"))
            {
                var query = (ConnectedServiceQuery)Enum.Parse(typeof(ConnectedServiceQuery), (string)message["query"]);
                switch (query)
                {
                    case ConnectedServiceQuery.CheckStatus:
                        returnMessage.Add("status", _status.ToString());
                        returnMessage.Add("success", "true");
                        break;
                    case ConnectedServiceQuery.StartHostingSession:
                        returnMessage.Add("success", StartHostingSessionHandle(appServiceConnection));
                        returnMessage.Add("status", _status.ToString());
                        break;
                    case ConnectedServiceQuery.StopHostingSession:
                        returnMessage.Add("success", StopHostingSessionHandle());
                        returnMessage.Add("status", _status.ToString());
                        break;
                    case ConnectedServiceQuery.MessageFromClient:
                        var hostListening = await HandleMessageFromClientAsync(message, returnMessage).ConfigureAwait(false);
                        returnMessage.Add("success", true);
                        returnMessage.Add("message_received", hostListening);
                        break;
                    case ConnectedServiceQuery.MessageFromHost:
                        var clientListening = await HandleMessageFromHostAsync(message, returnMessage).ConfigureAwait(false);
                        returnMessage.Add("success", true);
                        returnMessage.Add("message_received", clientListening);
                        break;
                }
            }
            else
            {
                returnMessage.Add("error", "message unknown");
            }

            return returnMessage;
        }

        /// <summary>
        /// Start the slideshow on the remote system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="deepLink"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToSystem(AdventureRemoteSystem system, string deepLink = "")
        {
            ValueSet message = new ValueSet();
            message.Add("query", ConnectedServiceQuery.StartHostingSession.ToString());

            var response = await system.SendMessage(message);

            if (response != null && response.ContainsKey("success") && (bool)response["success"])
            {
                var status = (ConnectedServiceStatus)Enum.Parse(typeof(ConnectedServiceStatus), (String)response["status"]);

                if (status != ConnectedServiceStatus.HostingNotConnected && status != ConnectedServiceStatus.HostingConnected)
                {
                    var launchUriStatus =
                        await RemoteLauncher.LaunchUriAsync(
                            new RemoteSystemConnectionRequest(system.RemoteSystem),
                            new Uri("adventure:" + deepLink)).AsTask().ConfigureAwait(false);

                    if (launchUriStatus != RemoteLaunchUriStatus.Success)
                    {
                        return false;
                    }
                }
                if (_remoteSystem != system)
                {
                    if (_remoteSystem != null)
                    {
                        _remoteSystem.MessageReceived -= _remoteSystem_MessageReceived;
                        _remoteSystem = null;
                    }
                    _remoteSystem = system;
                    _remoteSystem.MessageReceived += _remoteSystem_MessageReceived;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Initiates the second screen experience on the client if the system is showing a slideshow
        /// </summary>
        /// <param name="system"></param>
        /// <returns>Message from system with what adventure and current slide number</returns>
        public async Task<ValueSet> ConnectToSystem(AdventureRemoteSystem system)
        {
            ValueSet message = new ValueSet();
            message.Add("query", ConnectedServiceQuery.StartHostingSession.ToString());

            var response = await system.SendMessage(message);

            if (response != null && response.ContainsKey("success") && (bool)response["success"])
            {
                var status = (ConnectedServiceStatus)Enum.Parse(typeof(ConnectedServiceStatus), (String)response["status"]);

                if (status != ConnectedServiceStatus.HostingNotConnected && status != ConnectedServiceStatus.HostingConnected)
                {
                    return null;
                }

                message.Clear();

                if (_remoteSystem != system)
                {
                    if (_remoteSystem != null)
                    {
                        _remoteSystem.MessageReceived -= _remoteSystem_MessageReceived;
                        _remoteSystem = null;
                    }
                    _remoteSystem = system;
                    _remoteSystem.MessageReceived += _remoteSystem_MessageReceived;
                }

                response = await SendMessageFromClientAsync(message, SlideshowMessageTypeEnum.Status);

                if (response == null)
                {
                    _remoteSystem = null;
                    _remoteSystem.MessageReceived -= _remoteSystem_MessageReceived;
                    return null;
                }

                return response;
            }

            return null;
        }

        /// <summary>
        /// Sends message to discovered systems to find out if any are showing a slideshow
        /// </summary>
        /// <returns></returns>
        public async Task<List<AdventureRemoteSystem>> FindAllRemoteSystemsHostingAsync()
        {
            List<AdventureRemoteSystem> systems = new List<AdventureRemoteSystem>();
            var message = new ValueSet();
            message.Add("query", ConnectedServiceQuery.CheckStatus.ToString());

            foreach (var system in Rome.AvailableRemoteSystems)
            {
                var reponse = await system.SendMessage(message);
                if (reponse != null && reponse.ContainsKey("status"))
                {
                    var status = (ConnectedServiceStatus)Enum.Parse(typeof(ConnectedServiceStatus), (String)reponse["status"]);
                    if (status == ConnectedServiceStatus.HostingConnected || status == ConnectedServiceStatus.HostingNotConnected)
                    {
                        systems.Add(system);
                    }
                }
            }

            return systems;
        }

        /// <summary>
        /// Called from App to change status
        /// </summary>
        public void PrepareForBackground()
        {
            _previousStatus = _status;
            Status = ConnectedServiceStatus.IdleBackground;
        }

        /// <summary>
        /// Called from App to change status
        /// </summary>
        public void PrepareForForeground()
        {
            if (_status == ConnectedServiceStatus.IdleBackground)
                Status = _previousStatus;
        }

        /// <summary>
        /// Called from SlideshowPage once the user starts the slideshow
        /// </summary>
        public void StartHosting()
        {
            Status = _appServiceConnection != null ? ConnectedServiceStatus.HostingConnected : ConnectedServiceStatus.HostingNotConnected;
        }

        /// <summary>
        /// Called from SlideshowPage once the user exits the slideshow - changes the status and notifies the client
        /// </summary>
        public void StopHosting()
        {
            Status = ConnectedServiceStatus.IdleForeground;

            // send message to client to stop if connected
            if (_appServiceConnection != null)
            {
                ValueSet message = new ValueSet();
                message.Add("query", ConnectedServiceQuery.StopHostingSession.ToString());

                _appServiceConnection.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Send Message from Client to Host (such as ink data)
        /// </summary>
        public Task<ValueSet> SendMessageFromClientAsync(ValueSet message, SlideshowMessageTypeEnum queryType)
        {
            if (_remoteSystem == null || message == null)
            {
                return null;
            }

            message.Add("type", queryType.ToString());
            message.Add("query", ConnectedServiceQuery.MessageFromClient.ToString());
            return _remoteSystem.SendMessage(message);
        }

        /// <summary>
        /// Send Message from Host to Client (such as slide index update)
        /// </summary>
        public async Task<ValueSet> SendMessageFromHostAsync(ValueSet message, SlideshowMessageTypeEnum queryType)
        {
            if (_appServiceConnection == null)
            {
                return null;
            }

            message.Add("type", queryType.ToString());
            message.Add("query", ConnectedServiceQuery.MessageFromHost.ToString());
            var response = await _appServiceConnection.SendMessageAsync(message).AsTask().ConfigureAwait(false);

            if (response.Status == AppServiceResponseStatus.Success)
            {
                return response.Message;
            }
            else
            {
                return null;
            }
        }

        public static void DisposeInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }

        public void Dispose()
        {
            if (_appServiceConnection != null)
            {
                _appServiceConnection.Dispose();
                _appServiceConnection = null;
            }

            if (_remoteSystem != null)
            {
                _remoteSystem.Dispose();
                _remoteSystem = null;
            }

            Rome.Dispose();
        }

        /// <summary>
        /// Handle start hosting message received from client to host to start slideshow
        /// </summary>
        /// <param name="appServiceConnection"><see cref="AppServiceConnection"/> created when message was received from client</param>
        /// <returns>success</returns>
        private bool StartHostingSessionHandle(AppServiceConnection appServiceConnection)
        {
            bool success = false;
            try
            {
                if (_appServiceConnection != appServiceConnection)
                {
                    if (_appServiceConnection != null)
                    {
                        _appServiceConnection.ServiceClosed -= _appServiceConnection_ServiceClosed;
                        _appServiceConnection = null;
                    }

                    _appServiceConnection = appServiceConnection;
                    _appServiceConnection.ServiceClosed += _appServiceConnection_ServiceClosed;
                }


                if (Status == ConnectedServiceStatus.HostingNotConnected)
                    Status = ConnectedServiceStatus.HostingConnected;

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Handle stop hosting message received from host to client to stop controlling the slideshow
        /// </summary>
        /// <returns>success</returns>
        private bool StopHostingSessionHandle()
        {
            bool success = false;
            try
            {
                if (_appServiceConnection != null)
                {
                    _appServiceConnection.ServiceClosed -= _appServiceConnection_ServiceClosed;
                    _appServiceConnection = null;

                    if (Status == ConnectedServiceStatus.HostingConnected)
                        Status = ConnectedServiceStatus.HostingNotConnected;
                }

                if (_remoteSystem != null)
                {
                    _remoteSystem.MessageReceived -= _remoteSystem_MessageReceived;
                    _remoteSystem = null;

                    Status = ConnectedServiceStatus.IdleForeground;
                }

                HostingConnectionStoped?.Invoke(this, null);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Handle messages from client to host - FYI: SlideshowPage handles the events fired from here
        /// </summary>
        private async Task<bool> HandleMessageFromClientAsync(ValueSet message, ValueSet responseMessage)
        {
            if (!message.ContainsKey("type"))
            {
                responseMessage.Add("error", "type not found");
                return false;
            }

            var args = new SlideshowMessageReceivedEventArgs()
            {
                Message = message,
                ResponseMessage = responseMessage,
                QueryType = (SlideshowMessageTypeEnum)Enum.Parse(typeof(SlideshowMessageTypeEnum), (string)message["type"])
            };

            var taskCompletionSource = new TaskCompletionSource<object>();
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            var t = dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ReceivedMessageFromClient?.Invoke(this, args);
                taskCompletionSource.SetResult(null);
            });

            await taskCompletionSource.Task.ConfigureAwait(false);

            return ReceivedMessageFromClient != null;
        }

        /// <summary>
        /// Handle messages from host to client - FYI: SlideshowClientPage handles the events fired from here
        /// </summary>
        private async Task<bool> HandleMessageFromHostAsync(ValueSet message, ValueSet responseMessage)
        {
            if (!message.ContainsKey("type"))
            {
                responseMessage.Add("error", "type not found");
                return false;
            }

            var args = new SlideshowMessageReceivedEventArgs()
            {
                Message = message,
                ResponseMessage = responseMessage,
                QueryType = (SlideshowMessageTypeEnum)Enum.Parse(typeof(SlideshowMessageTypeEnum), (string)message["type"])
            };

            var taskCompletionSource = new TaskCompletionSource<object>();
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            var t = dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ReceivedMessageFromHost?.Invoke(this, args);
                taskCompletionSource.SetResult(null);
            });

            await taskCompletionSource.Task.ConfigureAwait(false);

            return ReceivedMessageFromHost != null;
        }

        private void _appServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _appServiceConnection.ServiceClosed -= _appServiceConnection_ServiceClosed;
            _appServiceConnection.Dispose();
            _appServiceConnection = null;

            if (Status == ConnectedServiceStatus.HostingConnected)
                Status = ConnectedServiceStatus.HostingNotConnected;
        }

        private void _remoteSystem_MessageReceived(object sender, AdventureRemoteSystemMessageReceivedEventArgs e)
        {
            HandleMessageReceivedAsync(e.Message, e.ResponseMessage);
        }
    }
}
