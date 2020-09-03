using SocketCore;
using SocketServer.Utils;
using System;
using System.Net.Sockets;

namespace SocketServer
{
    public class SocketClient<T> where T : IServerNetworkClient
    {
#if DEBUG
        public event ReceivePacketDebugInfo<ServerClient<T>> OnReceivePacket;
        public event SendPacketDebugInfo<ServerClient<T>> OnSendPacket;
#endif
        public ServerClient<T> Connect(ServerOptions<T> options)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                if (!client.ConnectAsync(options.IpAddress, options.Port).Wait(2000))
                    throw new Exception();

                var c = new ServerClient<T>(client, options);
#if DEBUG
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
#endif
                c.RunPacketReceiver();
                return c;
            }
            catch
            {

                return default(ServerClient<T>);
            }
        }
    }
}
