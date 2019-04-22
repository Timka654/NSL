using System;
using System.Collections.Generic;
using System.Text;
using SocketServer.Utils.Buffer;

namespace SocketServer.Utils.SystemPackets
{
    public class ServerTime
    {        
        public static void Send(INetworkClient client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)Enums.ClientPacketEnum.ServerTime
            };

            packet.WriteDateTime(DateTime.Now);

            client.Network.Send(packet);
        }
    }
}
