using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Utils.SystemPackets
{
    public class Version
    {
        public static void Send(BaseSocketNetworkClient client, long version)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ClientPacketEnum.Version
            };

            packet.WriteInt64(version);

            client.Network.Send(packet);
        }
    }
}
