using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    public class SocketClient<T> where T : IServerNetworkClient
    {
#if DEBUG
        public event ReceivePacketDebugInfo<T> OnReceivePacket;
        public event SendPacketDebugInfo<T> OnSendPacket;
#endif
        public Client<T> Connect(ServerOptions<T> options)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                if (!client.ConnectAsync(options.IpAddress, options.Port).Wait(2000))
                    throw new Exception();

                var c = new Client<T>(client, options);
#if DEBUG
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
#endif
                c.RunPacketReceiver();
                return c;
            }
            catch
            {

                return default(Client<T>);
            }
        }
    }
}
