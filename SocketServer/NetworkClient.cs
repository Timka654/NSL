using SocketCore.Utils;
using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.SystemPackets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer.Utils
{
    public class NetworkClient<TData> : Client<TData>
        where TData : IServerNetworkClient
    {
        ServerOptions<TData> serverOptions;

        Socket socket;

        public NetworkClient(ServerOptions<TData> options): base()
        {
            serverOptions = options;
        }

        public bool Connect()
        {
            socket = new Socket(serverOptions.AddressFamily, SocketType.Stream, serverOptions.ProtocolType);

            try
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(serverOptions.IpAddress), serverOptions.Port));

                Initialize(socket, serverOptions);
                RunPacketReceiver();
                return true;
            }
            catch
            {

            }
            return false;
        }
    }
}
