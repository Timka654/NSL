using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace SCL.Node.Packets
{
    internal class SignInPacket<TClient> : IPacket<TClient>
        where TClient : INodeNetworkClient
    {
        private readonly NodeHub<TClient> nodeHub;

        public SignInPacket(NodeHub<TClient> nodeHub)
        {
            this.nodeHub = nodeHub;
        }

        public override void Receive(TClient client, InputPacketBuffer data)
        {
            if (data.ReadString16() == nodeHub.ConnectionToken)
            {
                client.Dispose();
                return;
            }

            client.PlayerId = data.ReadString16();

            nodeHub.AddPlayer(client);
        }
    }
}
