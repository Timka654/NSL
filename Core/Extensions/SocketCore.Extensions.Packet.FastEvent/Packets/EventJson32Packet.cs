using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.SocketCore.Extensions.Packet.FastEvent.Packets
{
    public class EventJson32Packet<TClient, ReceiveType> : EventPacket<TClient, ReceiveType>
        where TClient : INetworkClient
    {
        public override void Receive(TClient client, InputPacketBuffer data)
        {
            InvokeEvent(client, data.ReadJson32<ReceiveType>());
        }
    }
}
