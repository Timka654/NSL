using System.Net;
using System;
using System.Net.Sockets;
using NSL.SocketClient;
using NSL.SocketCore;

namespace NSL.UDP.Client
{
    public class UDPNetworkClient<TClient> : UDPListener<TClient, UDPClientOptions<TClient>>
        where TClient : BaseSocketNetworkClient, new()
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
            if (!IPAddress.TryParse(options.IpAddress, out var ip))
                throw new ArgumentException($"invalid connection ip {options.IpAddress}", nameof(options.IpAddress));
            StartReceive(() => {
                client = new UDPClient<TClient>(options.GetIPEndPoint(), listener, options);

                client.OnReceivePacket += OnReceivePacket;
                client.OnSendPacket += OnSendPacket;

                options.InitializeClient(client.Data);
            });
        }

        public void Disconnect()
        {
            StopReceive();
        }

        protected override void Args_Completed(Span<byte> data, SocketReceiveFromResult e)
        {
            if (!state)
                return;

            RunReceiveAsync();

            if (e.RemoteEndPoint.Equals(options.GetIPEndPoint()))
                client.Receive(data);
        }
    }
}
