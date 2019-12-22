using SCL.SocketClient.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.SocketClient.Utils.SystemPackets
{
    public class Version
    {
        public static void Send(BaseSocketNetworkClient client, long version)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)Enums.ClientPacketEnum.Version
            };

            packet.WriteInt64(version);

            client.Network.Send(packet);
        }
    }
}
