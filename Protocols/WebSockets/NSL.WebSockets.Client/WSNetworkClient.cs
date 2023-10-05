using NSL.SocketClient;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;

namespace NSL.WebSockets.Client
{
    public class WSNetworkClient<T, TOptions> : WSClient<T>
        where T : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<T>
    {
        protected WebSocket client;

        public const int DefaultConnectionTimeout = 8_000;

        public WSClientOptions<T> Options => base.options as WSClientOptions<T>;

        public WSNetworkClient(TOptions options) : base(options)
        {
        }

        public bool Connect(string ip, int port, int connectionTimeOut = DefaultConnectionTimeout)
        {
            this.ConnectionOptions.IpAddress = ip;
            this.ConnectionOptions.Port = port;
            return Connect(connectionTimeOut);
        }

        public bool Connect(int connectionTimeOut = DefaultConnectionTimeout)
        {
            using (ManualResetEvent _lock = new ManualResetEvent(false))
            {
                Task.Run(async () =>
                {
                    if (await ConnectAsync(connectionTimeOut))
                        _lock.Set();
                });

                if (!_lock.WaitOne(connectionTimeOut))
                {
                    Release();
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> ConnectAsync(string ip, int port, int connectionTimeOut = DefaultConnectionTimeout)
        {
            this.ConnectionOptions.IpAddress = ip;
            this.ConnectionOptions.Port = port;

            return await ConnectAsync(connectionTimeOut);
        }

        public async Task<bool> ConnectAsync(int connectionTimeOut = DefaultConnectionTimeout)
        {
            if (base.disconnected == false)
                throw new InvalidOperationException("Client must be disconnected before reconnecting");

            try
            {
                return await ConnectProcess(connectionTimeOut);
            }
            catch (Exception ex)
            {
                Release();
                ConnectionOptions.RunException(ex);
                ConnectionOptions.RunClientDisconnect();
            }

            return false;
        }

        protected virtual async Task<bool> ConnectProcess(int connectionTimeOut)
        {
            client = CreateWS();

            CancellationTokenSource cts = new CancellationTokenSource();

            cts.CancelAfter(connectionTimeOut);

            await ConnectAsync(ConnectionOptions.EndPoint, cts.Token);

            return ProcessState(client.State);
        }

        protected virtual WebSocket CreateWS()
        {
            return new ClientWebSocket();
        }

        protected virtual async Task ConnectAsync(Uri endPoint, CancellationToken cts)
        {
            await ((ClientWebSocket)client).ConnectAsync(endPoint, cts);
        }

        protected bool ProcessState(WebSocketState state)
        {
            if (state == WebSocketState.Open)
            {
                Reconnect(client, Options.EndPoint);
                return true;
            }

            ConnectionOptions.RunClientDisconnect();

            return false;

        }

        private void Release()
        {
            if (client == null)
                return;

            client.Dispose();
            client = null;
        }
    }
}
