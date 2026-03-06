using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.SocketCore.Extensions.Packet.FastEvent.Packets
{
    public class EventJson16Packet<TClient, ReceiveType> : EventPacket<TClient, ReceiveType>
        where TClient : INetworkClient
    {
        public override void Receive(TClient client, InputPacketBuffer data)
        {
            InvokeEvent(client, data.ReadJson16<ReceiveType>());
        }
    }
}
