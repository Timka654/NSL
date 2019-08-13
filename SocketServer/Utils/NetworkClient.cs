using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using SocketServer.Utils.SystemPackets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer.Utils
{
    public class NetworkClient<TData> : Client<TData>
        where TData : INetworkClient
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

                serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.AliveConnection,
                    new AliveConnection<TData>());
                serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.RecoverySession,
                    new RecoverySession<TData>());
                serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.ServerTime,
                    new SocketServer.Utils.SystemPackets.SystemTime<TData>());
                serverOptions.AddPacket((ushort)Utils.SystemPackets.Enums.ClientPacketEnum.Version,
                    new Version<TData>());

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
