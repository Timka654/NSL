using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using NSL.UDP.Packet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NSL.UDP.Client
{
    public class UDPServer<TClient> : UDPListener<TClient, UDPClientOptions<TClient>>, INetworkListener
        where TClient : BaseNetworkConnection, new()
    {
        public event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceivePacket;

        public event SendPacketDebugInfo<UDPClient<TClient>> OnSendPacket;

        public UDPServer(UDPClientOptions<TClient> options) : base(options)
        {
            // Transparently echo the client connect-handshake probe so that
            // UDPNetworkClient.ConnectAsync / Connect(timeout) can detect server availability.
            //
            // Each connect attempt on the client generates a fresh session GUID which is embedded
            // in every probe packet.  The server compares it with the last acknowledged GUID:
            //   • Different GUID (new connect attempt or reconnect) → ReinitializeChannels + store new GUID.
            //   • Same  GUID  (repeated probe from the same attempt) → echo only, channels untouched.
            options.AddHandle(
                (ushort)NSLSystemPacketEnum.UDPConnectHandshake,
                (client, packet) =>
                {
                    var udpClient = client.Network as UDPClient<TClient>;
                    if (udpClient != null)
                    {
                        var sessionId = packet.ReadGuid();
                        if (sessionId != udpClient.LastHandshakeSessionId)
                        {
                            udpClient.LastHandshakeSessionId = sessionId;
                            udpClient.ReinitializeChannels();
                        }
                        //else
                        //{
                        //    options.CallExceptionEvent(new Exception($"[UDP-DIAG] Handshake probe ignored (same session GUID) ep={udpClient.GetRemotePoint()}"), null);
                        //}
                    }
                    client.Network.SendEmpty((ushort)NSLSystemPacketEnum.UDPConnectHandshake);
                });

            // Auto-register combined ping/pong handler: receives client pings and echoes back,
            // keeping both sides' AliveState alive without explicit calls in user code.
            options.RegisterUDPPingHandle();
        }

        public void Start()
        {
            base.StartReceive();
        }

        public void Stop()
        {
            StopReceive();
        }

        private ConcurrentDictionary<IPEndPoint, Lazy<UDPClient<TClient>>> clients = new ConcurrentDictionary<IPEndPoint, Lazy<UDPClient<TClient>>>();

        protected override void Options_OnClientDisconnectEvent(TClient client)
        {
            // Intentionally do NOT remove the dict entry here.
            //
            // Removing on disconnect races with Reinitialize(): if a packet arrives between
            // Disconnect() setting disconnected=true and RunDisconnect() TryRemove-ing the entry,
            // Reinitialize() reactivates the client, but the subsequent TryRemove kills the live
            // entry.  The next packet then creates a fresh UDPClient, firing a spurious connect
            // event — producing the constant connect/disconnect cycle observed with 2+ clients.
            //
            // With the entry retained, GetClient() will call Reinitialize() on the next arriving
            // packet and the client is seamlessly reactivated without any dict churn.
            // Stale entries (clients that never reconnect) can be cleaned up separately if needed.
        }

        protected override void Args_Completed(Span<byte> buffer, SocketReceiveFromResult e, CancellationToken token)
        {
            if (!state || ListenerCTS.IsCancellationRequested)
                return;

            RunReceiveIntern(token);

            GetClient(e.RemoteEndPoint as IPEndPoint)
                .Receive(buffer);
        }

        private UDPClient<TClient> GetClient(IPEndPoint endPoint)
        {
            var lazy = clients.GetOrAdd(endPoint, ipep =>
                new Lazy<UDPClient<TClient>>(() =>
                {
                    //options.CallExceptionEvent(new Exception($"[UDP-DIAG] GetClient: NEW UDPClient ep={ipep}"), null);
                    var client = new UDPClient<TClient>(ipep, listener, options);
                    client.OnReceivePacket += OnReceivePacket;
                    client.OnSendPacket += OnSendPacket;
                    return client;
                }));

            UDPClient<TClient> value;
            try
            {
                value = lazy.Value;
            }
            catch
            {
                clients.TryRemove(endPoint, out _);
                throw;
            }

            // If the client reconnected from the same endpoint before the server-side
            // AliveState eviction removed the entry, reinitialize the existing object instead
            // of replacing the dictionary entry (avoids netstandard compatibility issues).
            if (value.IsDisconnected)
            {
                //options.CallExceptionEvent(new Exception($"[UDP-DIAG] GetClient: IsDisconnected=true, calling Reinitialize ep={endPoint}"), null);
                value.Reinitialize();
            }

            return value;
        }

        public UDPClient<TClient> CreateClientConnection(IPEndPoint endPoint)
            => GetClient(endPoint);

        public int GetListenerPort() => options.GetBindingEndPoint().Port;

        public CoreOptions GetOptions() => options;
    }
}
