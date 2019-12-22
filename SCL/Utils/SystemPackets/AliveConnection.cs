using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketCore.Utils.Buffer;
using SocketServer.Utils.SystemPackets.Enums;

namespace SCL.Utils.SystemPackets
{
    public class AliveConnection<T> : IPacket<T> where T: BaseSocketNetworkClient
    {
        protected override void Receive(InputPacketBuffer data)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort) ClientPacketEnum.AliveConnection
            };
            Send(packet);
        }

        public AliveConnection(ClientOptions<T> options) : base(options)
        {
        }
    }
}
