using System.Net;
using System;
using System.Net.Sockets;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System.Threading;

namespace NSL.UDP.Client
{
    public class UDPNetworkClient<TClient> : UDPListener<TClient, UDPClientOptions<TClient>>
        where TClient : BaseNetworkConnection, new()
    {
        public event ReceivePacketDebugInfo<UDPClient<TClient>> OnReceivePacket;
        public event SendPacketDebugInfo<UDPClient<TClient>> OnSendPacket;

        UDPClient<TClient> client;

        public UDPClient<TClient> GetClient() => client;

        public UDPNetworkClient(UDPClientOptions<TClient> options): base(options)
        {
        }

        public void Connect()
        {
            var remoteEp = options.GetRemoteEndPoint();

            if (!IPAddress.TryParse(remoteEp.IpAddress, out _))
                throw new ArgumentException($"invalid connection ip {remoteEp.IpAddress}", nameof(remoteEp.IpAddress));

            StartReceive(() => {
                client = new UDPClient<TClient>(remoteEp.GetIPEndPoint(), listener, options, deferConnect: true);

                client.OnReceivePacket += OnReceivePacket;
                client.OnSendPacket += OnSendPacket;
            });
        }

        public void Disconnect()
        {
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
