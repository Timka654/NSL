using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    public class SocketClient<T> where T : INetworkClient
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
                
#if DEBUG
                var c = new Client<T>(client, options);
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
                c.RunPacketReceiver();
                return c;
#else
               return new Client<T>(client, serverOptions).RunPacketReceiver();
#endif
            }
            catch
            {

                return default(Client<T>);
            }
        }
    }
}
