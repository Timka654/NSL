using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using System.Linq;

namespace NSL.SocketClient.Utils
{
    public enum RecoverySessionResultEnum
    {
        Ok,
        NotFound,
        NoData
    }
}

namespace NSL.SocketClient.Utils.SystemPackets
{
    public class RecoverySessionPacket<T> : IPacketMessage<T, RecoverySessionResultEnum> where T : BaseSocketNetworkClient
    {
        internal static RecoverySessionPacket<T> Instance;

        public RecoverySessionPacket(ClientOptions<T> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            var result = (RecoverySessionResultEnum)data.ReadByte();
            
            if (result == RecoverySessionResultEnum.Ok)
            {
                string session = data.ReadString16();
                var recoverySessionKeyArray = new string[data.ReadInt32()];
                for (int i = 0; i < Client.RecoverySessionKeyArray.Count(); i++)
                {
                    recoverySessionKeyArray[i] = data.ReadString16();
                }

                Client.SetRecoveryData(session, recoverySessionKeyArray);
            }

            InvokeEvent(result);
        }

        public static void Send(BaseSocketNetworkClient client)
        {
            if (client.RecoverySessionKeyArray == null || client.Session == null)
            {
                Instance.InvokeEvent(RecoverySessionResultEnum.NoData);
                return; 
            }

            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ServerPacketEnum.RecoverySession
            };

            packet.WriteString16(client.Session);

            packet.WriteInt32(client.RecoverySessionKeyArray.Length);

            foreach (var item in client.RecoverySessionKeyArray)
            {
                packet.WriteString16(item);
            }

            client.Network.Send(packet);
        }
    }
}
