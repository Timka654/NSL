using System.Net;
using System;
using System.Net.Sockets;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using NSL.UDP.Packet;
using System.Threading;
using System.Threading.Tasks;
namespace NSL.UDP.Client
{
    public class UDPNetworkClient<TClient> : UDPListener<TClient, UDPClientOptions<TClient>>
        where TClient : BaseNetworkConnection, new()
    {
        public const int DefaultConnectionTimeout = 8_000;

        public event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceivePacket;
        public event SendPacketDebugInfo<UDPClient<TClient>> OnSendPacket;

        UDPClient<TClient> client;

        public UDPClient<TClient> GetClient() => client;

        /// <summary>
        /// Generated once per <see cref="ConnectCore"/> call and embedded in every handshake probe.
        /// The server compares it to the last acknowledged GUID to decide whether to reinitialize
        /// channels (new session) or just echo (repeated probe from the same attempt).
        /// </summary>
        private Guid _sessionId;

        public UDPNetworkClient(UDPClientOptions<TClient> options): base(options)
        {
            // Auto-register combined ping/pong handler so the alive-check loop works
            // without requiring explicit RegisterUDPPingHandle() calls in user code.
            options.RegisterUDPPingHandle();
        }

        /// <summary>
        /// Starts the UDP socket without waiting for a connection confirmation.
        /// Use <see cref="Connect(int)"/> or <see cref="ConnectAsync(int)"/> to get success/failure feedback.
        /// </summary>
        public void Connect()
        {
            ConnectCore();
        }

        /// <summary>
        /// Starts the UDP socket and blocks until the server acknowledges the connection
        /// (via <see cref="NSLSystemPacketEnum.UDPConnectHandshake"/> echo) or
        /// <paramref name="connectionTimeOut"/> milliseconds elapse.
        /// </summary>
        /// <returns><c>true</c> if connected successfully; otherwise <c>false</c>.</returns>
        public bool Connect(int connectionTimeOut)
        {
            using var mre = new ManualResetEventSlim(false);

            CoreOptions.ClientConnect onConnected = null;
            onConnected = _ =>
            {
                options.OnClientConnectEvent -= onConnected;
                mre.Set();
            };
            options.OnClientConnectEvent += onConnected;

            try
            {
                ConnectCore();
            }
            catch
            {
                options.OnClientConnectEvent -= onConnected;
                throw;
            }

            // Periodically probe the server so it learns about this client.
            using var probeCts = new CancellationTokenSource(connectionTimeOut);
            _ = SendHandshakeProbesAsync(probeCts.Token);

            if (!mre.Wait(connectionTimeOut))
            {
                options.OnClientConnectEvent -= onConnected;
                Disconnect();
                return false;
            }

            probeCts.Cancel();
            return true;
        }

        /// <summary>
        /// Starts the UDP socket and asynchronously waits until the server acknowledges the connection
        /// (via <see cref="NSLSystemPacketEnum.UDPConnectHandshake"/> echo) or
        /// <paramref name="connectionTimeOut"/> milliseconds elapse.
        /// </summary>
        /// <returns><c>true</c> if connected successfully; otherwise <c>false</c>.</returns>
        public async Task<bool> ConnectAsync(int connectionTimeOut = DefaultConnectionTimeout)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            CoreOptions.ClientConnect onConnected = null;
            onConnected = _ =>
            {
                options.OnClientConnectEvent -= onConnected;
                tcs.TrySetResult(true);
            };
            options.OnClientConnectEvent += onConnected;

            try
            {
                ConnectCore();
            }
            catch
            {
                options.OnClientConnectEvent -= onConnected;
                throw;
            }

            // Periodically probe the server so it learns about this client.
            using var probeCts = new CancellationTokenSource(connectionTimeOut);
            _ = SendHandshakeProbesAsync(probeCts.Token);

            probeCts.Token.Register(() => tcs.TrySetResult(false));

            if (!await tcs.Task)
            {
                options.OnClientConnectEvent -= onConnected;
                Disconnect();
                return false;
            }

            probeCts.Cancel();
            return true;
        }

        private async Task SendHandshakeProbesAsync(CancellationToken token)
        {
            const int probeIntervalMs = 200;
            while (!token.IsCancellationRequested)
            {
                // Use Unreliable channel: no ACK, no retransmit, no ordering state accumulated.
                // The probe loop itself handles retries, so reliability is not needed here.
                var c = client;
                if (c != null)
                {
                    var probe = new DgramOutputPacketBuffer
                    {
                        PacketId = (ushort)NSLSystemPacketEnum.UDPConnectHandshake,
                        Channel = UDPChannelEnum.Unreliable | UDPChannelEnum.Unordered
                    };
                    probe.WriteGuid(_sessionId);
                    c.Send(probe);
                }
                try { await Task.Delay(probeIntervalMs, token); }
                catch (OperationCanceledException) { break; }
            }
        }

        private void ConnectCore()
        {
            // New connect attempt → new session GUID.  Must be set before StartReceive so that
            // the first probe packet always carries the fresh GUID.
            _sessionId = Guid.NewGuid();
            // If the socket is already running (e.g. UDPClient.Disconnect() fired from internal
            // Sync/AliveState check, but UDPNetworkClient.Disconnect() was never called),
            // stop the listener cleanly so StartReceive creates a fresh socket and a new UDPClient
            // with deferConnect:true.  This is necessary for reconnect to work correctly.
            if (state)
            {
                client = null;
                StopReceive();
            }

            var remoteEp = options.GetRemoteEndPoint();

            if (!IPAddress.TryParse(remoteEp.IpAddress, out _))
                throw new ArgumentException($"invalid connection ip {remoteEp.IpAddress}", nameof(remoteEp.IpAddress));

            StartReceive(() =>
            {
                client = new UDPClient<TClient>(remoteEp.GetIPEndPoint(), listener, options, deferConnect: true);

                client.OnReceivePacket += OnReceivePacket;
                client.OnSendPacket += OnSendPacket;
            });
        }

        public void Disconnect()
        {
            var c = client;
            client = null;
            c?.Disconnect();
            StopReceive();
        }

        protected override void Args_Completed(Span<byte> data, SocketReceiveFromResult e, CancellationToken token)
        {
            if (!state)
                return;

            RunReceiveIntern(token);

            if (e.RemoteEndPoint.Equals(options.GetRemoteEndPoint().GetIPEndPoint()))
                client.Receive(data);
        }
    }
}
