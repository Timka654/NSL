using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.Packet.FastEvent
{
    public class EventJson32Packet<TClient, ReceiveType> : EventPacket<TClient, ReceiveType>
        where TClient : INetworkClient
    {
        public override void Receive(TClient client, InputPacketBuffer data)
            => InvokeEvent(client, data.ReadJson32<ReceiveType>());
    }
}
