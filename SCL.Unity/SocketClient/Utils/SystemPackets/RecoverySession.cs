using SCL.SocketClient.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.SocketClient.Utils
{
    public enum RecoverySessionResultEnum
    {
        Ok,
        NotFound
    }
}

namespace SCL.SocketClient.Utils.SystemPackets
{
    public class RecoverySession<T> : IPacketMessage<T, RecoverySessionResultEnum> where T : BaseSocketNetworkClient
    {
        public RecoverySession(ClientOptions<T> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            InvokeEvent((RecoverySessionResultEnum)data.ReadByte());
        }

        public static void Send(BaseSocketNetworkClient client)
        {
            if (client.RecoverySessionKeyArray == null)
                return;

            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)Enums.ClientPacketEnum.RecoverySession
            };

            packet.WriteInt32(client.RecoverySessionKeyArray.Length);

            foreach (var item in client.RecoverySessionKeyArray)
            {
                packet.WriteString16(item);
            }

            client.Network.Send(packet);
        }
    }
}
