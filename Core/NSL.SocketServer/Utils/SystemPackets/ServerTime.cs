using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using System;

namespace SocketServer.Utils.SystemPackets
{
    public class SystemTime<T> : IPacket<T> where T : IServerNetworkClient
    {
        public event OnRecoverySessionReceiveDelegate<T> OnRecoverySessionReceiveEvent;

        public SystemTime()
        {
        }

        public override void Receive(T client, InputPacketBuffer data)
        {
            Send(client);
        }

        public static void Send(IServerNetworkClient client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ClientPacketEnum.ServerTimeResult
            };

            packet.WriteDateTime(DateTime.UtcNow);

            client.Network.Send(packet);
        }
    }
}