using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace SocketPhantom.AspNetCore.Network.Packets
{
    internal class InvokePacket : IPacket<PhantomHubClientProxy>
    {
        public override void Receive(PhantomHubClientProxy client, InputPacketBuffer data)
        {
            client.Hub.Invoke(client, data);
        }
    }
}
