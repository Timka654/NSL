using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SCL.SocketClient.Utils.Buffer;

namespace SCL.SocketClient.Utils.SystemPackets
{
    public class AliveConnection<T> : IPacket<T> where T: BaseSocketNetworkClient
    {
        protected override void Receive(InputPacketBuffer data)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort) Enums.ClientPacketEnum.AliveConnection
            };
            Send(packet);
        }

        public AliveConnection(ClientOptions<T> options) : base(options)
        {
        }
    }
}
