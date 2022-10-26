using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsClient.Unity;
using NSL.SocketPhantom.Unity.Network.Packets;
using NSL.WebSockets.Client;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using static NSL.SocketPhantom.Unity.PhantomHubConnection;

namespace NSL.SocketPhantom.Unity.Network
{
    internal class PhantomNetworkClient
    {
        internal WSNetworkClient<PhantomSocketNetworkClient, WSClientOptions<PhantomSocketNetworkClient>> client;

        public async Task InitializeClient(PhantomRequestResult data)
        {
            var connectUrl = new Uri(await phantomHubConnection.Options.Url());

            phantomHubConnection.DebugException($"Initialize client with host {connectUrl}");

            var builder = WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<PhantomSocketNetworkClient>()
                .WithOptions<WSClientOptions<PhantomSocketNetworkClient>>()
                .WithUrl(connectUrl)
                .WithCode(_ =>
                {
                    _.AddConnectHandle(ClientOptions_OnClientConnectEvent);
                    _.AddDisconnectHandle(ClientOptions_OnClientDisconnectEvent);
                    _.AddExceptionHandle(ClientOptions_OnExceptionEvent);

                    _.AddPacket(1, new SessionPacket(phantomHubConnection));
                    _.AddPacket(2, new InvokePacket());
                });

            client = Application.platform == RuntimePlatform.WebGLPlayer ? builder.BuildForWGLPlatform() : builder.Build();

            bool oldConnectionState = SuccessConnected;

            SuccessConnected = false;

            if (!await client.ConnectAsync((int)phantomHubConnection.ConnectionTimeout.TotalMilliseconds) && oldConnectionState)
            {
                phantomHubConnection.DebugException($"Failed connected - {connectUrl}");
                SuccessConnected = true;
                ClientOptions_OnClientDisconnectEvent(null);
                return;
            }

            phantomHubConnection.DebugException($"Success connected {connectUrl}");
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
