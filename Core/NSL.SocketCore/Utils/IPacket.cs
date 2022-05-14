using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils
{
    public abstract class IPacket<TClient> 
        where TClient : INetworkClient
    {
       public abstract void Receive(TClient client, InputPacketBuffer data);
    }
}
