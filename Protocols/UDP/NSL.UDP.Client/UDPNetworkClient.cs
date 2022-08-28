using NSL.SocketCore;
using NSL.SocketServer.Utils;
using System.Net;
using System;
using System.Net.Sockets;
using NSL.SocketClient;

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
            });
        }

        protected override void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!state)
                return;

            RunReceiveAsync(ListenerCTS.Token);

            if (e.RemoteEndPoint.Equals(options.GetIPEndPoint()))
                client.Receive(e);
            else
                e.Dispose();
        }
    }
}
