using Microsoft.AspNetCore.SignalR;
using SocketCore.Extensions.Buffer;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketPhantom.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.AspNetCore.Network.Packets
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

            var sessionResultPacket = new OutputPacketBuffer<PacketEnum>();

            sessionResultPacket.PacketId = PacketEnum.SignInResult;

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
