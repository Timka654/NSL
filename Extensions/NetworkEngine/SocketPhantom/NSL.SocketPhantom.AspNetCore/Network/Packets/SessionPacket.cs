using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketPhantom.Enums;

namespace NSL.SocketPhantom.AspNetCore.Network.Packets
{
    internal class SessionPacket : IPacket<PhantomHubClientProxy>
    {
        PhantomHubsManager manager;
        public SessionPacket(PhantomHubsManager manager)
        {
            this.manager = manager;
        }

        public override void Receive(PhantomHubClientProxy client, InputPacketBuffer data)
        {
            var path = data.ReadString16();

            client.Session = data.ReadString16();

            var sessionResultPacket = OutputPacketBuffer.Create(PacketEnum.SignInResult);

            if (manager.ProcessClient(client, path, out var hub))
                sessionResultPacket.WriteByte(byte.MaxValue);
            else if (hub == null)
                sessionResultPacket.WriteByte((byte)SignStatusCodeEnum.ErrorPath);
            else
                sessionResultPacket.WriteByte((byte)SignStatusCodeEnum.ErrorSession);

            client.Send(sessionResultPacket);
        }
    }
}
