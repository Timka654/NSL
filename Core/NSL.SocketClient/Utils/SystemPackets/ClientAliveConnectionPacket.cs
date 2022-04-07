using SocketCore.Utils.Buffer;

namespace SocketClient.Utils.SystemPackets
{
    public class ClientAliveConnectionPacket<T> : IClientPacket<T> where T: BaseSocketNetworkClient
    {
        protected override void Receive(InputPacketBuffer data)
        {
            Client.PongProcess();
        }

        public ClientAliveConnectionPacket(ClientOptions<T> options) : base(options)
        {
        }
    }
}
