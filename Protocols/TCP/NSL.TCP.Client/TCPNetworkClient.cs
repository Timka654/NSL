using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.TCP.Client
{
    /// <summary>Типизированная обёртка первого уровня. Хранит ClientOptions&lt;T&gt; и обеспечивает typed доступ.</summary>
    public class TCPNetworkClient<T> : TCPNetworkClient
        where T : BaseNetworkConnection, new()
    {
        public new T Data => (T)base.options.ClientData;

        public TCPNetworkClient(CoreOptions options, bool legacyThread = false) : base(options, legacyThread) { }
    }

    /// <summary>Не-дженериковый движок TCP-клиента с логикой подключения.</summary>
    public class TCPNetworkClient : TCPClient
    {
        public const int DefaultConnectionTimeout = 8_000;

        public TCPNetworkClient(CoreOptions options, bool legacyThread = false) : base(options, legacyThread)
        {
        }

        public bool Connect(string ip, int port, int connectionTimeOut = DefaultConnectionTimeout)
        {
            ConnectionOptions.WithRemoteEndPoint(ip, port);
            return Connect(connectionTimeOut);
        }

        public bool Connect(int connectionTimeOut = DefaultConnectionTimeout)
        {
            using (var _lock = new ManualResetEvent(false))
            {
                Task.Run(async () => { if (await ConnectAsync()) _lock.Set(); });
                if (!_lock.WaitOne(connectionTimeOut)) { Release(); return false; }
                return true;
            }
        }

        public async Task<bool> ConnectAsync(string ip, int port, int connectionTimeOut = DefaultConnectionTimeout)
        {
            ConnectionOptions.WithRemoteEndPoint(ip, port);
            return await ConnectAsync(connectionTimeOut);
        }

        public async Task<bool> ConnectAsync(int connectionTimeOut = DefaultConnectionTimeout)
        {
            if (base.disconnected == false)
                throw new InvalidOperationException("Client must be disconnected before reconnecting");

            try
            {
                var remoteEp = ConnectionOptions.GetRemoteEndPoint();

                if (!IPAddress.TryParse(remoteEp.IpAddress, out var ip))
                    throw new ArgumentException($"invalid connection ip {remoteEp.IpAddress}", nameof(remoteEp.IpAddress));

                if (ConnectionOptions.AddressFamily == AddressFamily.Unspecified)
                    ConnectionOptions.AddressFamily = ip.AddressFamily;

                if (ConnectionOptions.ProtocolType == ProtocolType.Unspecified)
                    ConnectionOptions.ProtocolType = ProtocolType.Tcp;

                var client = new Socket(ConnectionOptions.AddressFamily, SocketType.Stream, ConnectionOptions.ProtocolType);
                client.ReceiveBufferSize = ConnectionOptions.ReceiveBufferSize;
                client.NoDelay = true;

                var connectTask = client.ConnectAsync(ip, remoteEp.Port);

                if (await Task.WhenAny(connectTask, Task.Delay(connectionTimeOut)) != connectTask)
                {
                    connectTask.ContinueWith(t => { _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                    throw new TaskCanceledException();
                }

                await connectTask;

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
        }

        private Socket _connectingSocket;

        private void Release()
        {
            _connectingSocket?.Dispose();
            _connectingSocket = null;
        }
    }
}
