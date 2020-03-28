using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using System;

namespace SocketServer.Utils.SystemPackets
{
    public class SystemTime
    {
        public static void Send(IServerNetworkClient client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ClientPacketEnum.ServerTime
            };

            packet.WriteDateTime(DateTime.Now);

            client.Network.Send(packet);
        }
    }
}
