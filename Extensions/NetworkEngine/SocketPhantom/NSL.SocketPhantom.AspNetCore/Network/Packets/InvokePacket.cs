using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketPhantom.AspNetCore.Network.Packets
{
    internal class InvokePacket : IPacket<PhantomHubClientProxy>
    {
        public override void Receive(PhantomHubClientProxy client, InputPacketBuffer data)
        {
            client.Hub.Invoke(client, data);
        }
    }
}
