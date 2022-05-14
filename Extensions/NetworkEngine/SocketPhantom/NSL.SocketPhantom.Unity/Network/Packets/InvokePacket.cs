using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketPhantom.Unity.Network.Packets
{
    internal class InvokePacket : IPacket<PhantomSocketNetworkClient>
    {
        public override void Receive(PhantomSocketNetworkClient client, InputPacketBuffer data)
        {
            client.connection.Invoke(data);
        }
    }
}
