using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Utils.Buffer;

namespace SocketServer.Utils
{
    public enum RecoverySessionResultEnum
    {
        Ok,
        NotFound
    }
}

namespace SocketServer.Utils.SystemPackets
{
    public class RecoverySession<T> :IPacket<T> where T: INetworkClient
    {
        public void Receive(T client, InputPacketBuffer data)
        {
        }

        public static void Send(INetworkClient client, RecoverySessionResultEnum result)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)Enums.ServerPacketEnum.RecoverySessionResult
            };

            packet.WriteByte((byte)result);

            client.Send(packet);
        }
    }
}
