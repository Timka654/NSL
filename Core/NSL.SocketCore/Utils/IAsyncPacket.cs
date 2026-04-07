using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils
{
    public abstract class IAsyncPacket<TClient> : IPacket<TClient>
        where TClient : BaseNetworkConnection
    {
        public override void Receive(TClient client, InputPacketBuffer data)
        {
            ReceiveAsync(client, data).Wait();
        }

        public abstract Task ReceiveAsync(TClient client, InputPacketBuffer data);
    }
}
