using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils
{
    public abstract class IPacket
    {
        public abstract void Receive(BaseNetworkConnection client, InputPacketBuffer data);
    }

    public abstract class IPacket<TClient> : IPacket
        where TClient : BaseNetworkConnection
    {
        public abstract void Receive(TClient client, InputPacketBuffer data);

        public override void Receive(BaseNetworkConnection client, InputPacketBuffer data)
            => Receive((TClient)client, data);
    }
}