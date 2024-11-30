using NSL.SocketClient;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.TCP.Client
{
    public class TCPNetworkClient<T> : TCPNetworkClient<T, ClientOptions<T>>
        where T : BaseSocketNetworkClient, new()
    {
        public TCPNetworkClient(ClientOptions<T> options, bool legacyThread = false) : base(options, legacyThread)
        {
        }
    }

    public class TCPNetworkClient<T, TOptions> : TCPClient<T>
        where T : BaseSocketNetworkClient, new()
        where TOptions : ClientOptions<T>
    {
        Socket client;

        public const int DefaultConnectionTimeout = 8_000;

        public TCPNetworkClient(TOptions options, bool legacyThread = false) : base(options, legacyThread)
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

            //return await Task.Run(() =>
            //{
            try
            {
                if (!IPAddress.TryParse(ConnectionOptions.IpAddress, out var ip))
                    throw new ArgumentException($"invalid connection ip {ConnectionOptions.IpAddress}", nameof(ConnectionOptions.IpAddress));

                if (ConnectionOptions.AddressFamily == AddressFamily.Unspecified)
                    ConnectionOptions.AddressFamily = ip.AddressFamily;

                if (ConnectionOptions.ProtocolType == ProtocolType.Unspecified)
                    ConnectionOptions.ProtocolType = ProtocolType.Tcp;

                client = new Socket(ConnectionOptions.AddressFamily, SocketType.Stream, ConnectionOptions.ProtocolType);

                client.ReceiveBufferSize = ConnectionOptions.ReceiveBufferSize;

                client.NoDelay = true;

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                //await client.ConnectAsync(ConnectionOptions.IpAddress, ConnectionOptions.Port);
                cancellationTokenSource.CancelAfter(connectionTimeOut);

                await Task.Run(() => client.ConnectAsync(ip, ConnectionOptions.Port), cancellationTokenSource.Token);

                Reconnect(client);

                return true;
            }
            catch (TaskCanceledException)
            {
                Release();
                ConnectionOptions.RunClientDisconnect();
            }
            catch (Exception ex)
            {
                Release();
                ConnectionOptions.RunException(ex);
                ConnectionOptions.RunClientDisconnect();
            }

            return false;
            //});
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
