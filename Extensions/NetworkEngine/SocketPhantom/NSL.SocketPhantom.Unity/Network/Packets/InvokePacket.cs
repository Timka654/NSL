using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace SocketPhantom.Unity.Network.Packets
{
    internal class InvokePacket : IPacket<PhantomSocketNetworkClient>
    {
        public override void Receive(PhantomSocketNetworkClient client, InputPacketBuffer data)
        {
            client.connection.Invoke(data);
        }
    }
}
