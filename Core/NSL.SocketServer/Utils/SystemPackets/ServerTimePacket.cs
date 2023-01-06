using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketServer.Utils.SystemPackets
{
    public class SystemTime<T> : IPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 1;

        public SystemTime()
        {
        }

        public override void Receive(T client, InputPacketBuffer data)
        {
            Send(client, data.ReadDateTime());
        }

        private static void Send(IServerNetworkClient client, DateTime clientTime)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            packet.WriteDateTime(clientTime);

            packet.WriteDateTime(DateTime.UtcNow);

            client.Network.Send(packet);
        }
    }
}