using NSL.SocketClient;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.WebSockets;

namespace NSL.WebSockets.Client
{
    internal class WSNetworkClient<T, TOptions> : WSClient<T>
        where T : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<T>
    {
        ClientWebSocket client;

        public const int DefaultConnectionTimeout = 8_000;

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
            ManualResetEvent _lock = new ManualResetEvent(false);

            Task.Run(async () =>
            {
                if (await ConnectAsync())
                    _lock.Set();
            });

            if (!_lock.WaitOne(connectionTimeOut))
            {
                Release();
                return false;
            }

            return true;
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


            var options = (WSClientOptions<T>)base.options;
            try
            {
                client = new ClientWebSocket();

                CancellationTokenSource cts = new CancellationTokenSource();

                cts.CancelAfter(DefaultConnectionTimeout);

                await client.ConnectAsync(options.EndPoint, cts.Token);
                //await client.ConnectAsync(ConnectionOptions.IpAddress, ConnectionOptions.Port);

                var success = client.State == WebSocketState.Open;

                if (success)
                {
                    Reconnect(client, options.EndPoint);
                }
                else
                {
                    ConnectionOptions.RunClientDisconnect();
                }

                return success;
            }
            catch (Exception ex)
            {
                Release();
                ConnectionOptions.RunException(ex);
                ConnectionOptions.RunClientDisconnect();
            }

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
