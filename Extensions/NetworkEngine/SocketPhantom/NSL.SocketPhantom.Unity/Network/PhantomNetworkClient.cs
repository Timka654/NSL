using NSL.SocketClient;
using NSL.TCP.Client;
using NSL.SocketPhantom.Unity.Network.Packets;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static NSL.SocketPhantom.Unity.PhantomHubConnection;

namespace NSL.SocketPhantom.Unity.Network
{
    internal class PhantomNetworkClient
    {
        ClientOptions<PhantomSocketNetworkClient> clientOptions;

        internal TCPNetworkClient<PhantomSocketNetworkClient, ClientOptions<PhantomSocketNetworkClient>> client;

        public async Task InitializeClient(PhantomRequestResult data)
        {
            var connectUrl = new Uri(data.Url);

            phantomHubConnection.DebugException($"Initialize client with host {data.Url}");

            var dnss = Dns.GetHostAddresses(connectUrl.Host);

            phantomHubConnection.DebugException($"Founded {dnss.Length} dns count - {string.Join(",", dnss.Select(x => x.ToString()))}");

            var dns = dnss.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) ?? dnss.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);

            if (dns == null)
                throw new Exception($"dns for {data.Url} not found");

            phantomHubConnection.DebugException($"Selected IPAddress = {dns}");

            clientOptions = new ClientOptions<PhantomSocketNetworkClient>();

            var ip = IPAddress.Parse(dns.ToString());

            clientOptions.OnClientConnectEvent += ClientOptions_OnClientConnectEvent;
            clientOptions.OnClientDisconnectEvent += ClientOptions_OnClientDisconnectEvent;
            clientOptions.OnExceptionEvent += ClientOptions_OnExceptionEvent;

            clientOptions.AddPacket(1, new SessionPacket(phantomHubConnection));
            clientOptions.AddPacket(2, new InvokePacket());

            phantomHubConnection.Options.CipherProvider.SetProvider(clientOptions);

            client = new TCPNetworkClient<PhantomSocketNetworkClient, ClientOptions<PhantomSocketNetworkClient>>(clientOptions);

            bool oldConnectionState = SuccessConnected;

            SuccessConnected = false;

            if (!await client.ConnectAsync(ip.ToString(),
                connectUrl.Port,
                (int)phantomHubConnection.ConnectionTimeout.TotalMilliseconds) && oldConnectionState)
            {
                phantomHubConnection.DebugException($"Failed connected - {ip.ToString()}:{connectUrl.Port}");
                SuccessConnected = true;
                ClientOptions_OnClientDisconnectEvent(null);
                return;
            }

            phantomHubConnection.DebugException($"Success connected {ip.ToString()}:{connectUrl.Port}");
        }

        internal int retryCount = 0;

        private PhantomHubConnection phantomHubConnection;

        public event Action<Exception, PhantomSocketNetworkClient> OnException = (e, c) => { };

        public PhantomNetworkClient(PhantomHubConnection phantomHubConnection)
        {
            this.phantomHubConnection = phantomHubConnection;
        }

        private async void ClientOptions_OnClientDisconnectEvent(PhantomSocketNetworkClient client)
        {
            SuccessConnected = CanReconnection();

            if (SuccessConnected)
            {
                var policy = await phantomHubConnection.Options.RetryPolicy();

                if (policy != null)
                {
                    var elapse = policy.NextRetryDelay(new RetryContext()
                    {
                        ElapsedTime = TimeSpan.Zero,
                        PreviousRetryCount = retryCount,
                        RetryReason = null
                    });

                    retryCount++;

                    if (elapse.HasValue)
                    {
                        phantomHubConnection.SetState(HubConnectionState.Reconnecting);

                        while (CanReconnection() && !this.client.GetState())
                        {
                            await Task.Delay(elapse.Value);

                            await phantomHubConnection.StartAsync(true);
                        }

                        if (this.client.GetState())
                            return;
                    }
                }
            }

            phantomHubConnection.SetState(HubConnectionState.Disconnected);
            phantomHubConnection.ForceStop(null);
        }

        private bool SuccessConnected = false;

        private bool CanReconnection() =>
            SuccessConnected &&
            !phantomHubConnection.ForceStoppedState &&
            phantomHubConnection.Options.RetryPolicy != null;

        private void ClientOptions_OnClientConnectEvent(PhantomSocketNetworkClient client)
        {
            SuccessConnected = client.PingPongEnabled = true;

            client.connection = phantomHubConnection;

            SessionPacket.Send(client, phantomHubConnection.Path, phantomHubConnection.Session);
        }

        private void ClientOptions_OnExceptionEvent(Exception ex, PhantomSocketNetworkClient client) => OnException(ex, client);
    }
}
