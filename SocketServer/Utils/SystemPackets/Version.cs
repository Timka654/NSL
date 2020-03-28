using SocketCore.Utils.Buffer;

namespace SocketServer.Utils.SystemPackets
{
    public class Version<T> : IPacket<T> where T : IServerNetworkClient
    {
        public void Receive(T client, InputPacketBuffer data)
        {
            client.Version = data.ReadInt64();
        }
    }
}
