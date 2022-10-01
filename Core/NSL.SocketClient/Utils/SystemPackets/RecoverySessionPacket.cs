using NSL.SocketCore.Utils.Buffer;
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
    public class RecoverySessionPacket
    {
        public const ushort PacketId = ushort.MaxValue - 2;

        public static void Send(BaseSocketNetworkClient client)
        {
            if (client.Session == default && client.RecoverySessionKeyArray == default)
                return;

            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            packet.WriteString16(client.Session);

            packet.WriteCollection(client.RecoverySessionKeyArray, packet.WriteString16);

            client.Network.Send(packet);
        }
    }
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

                var recoverySessionKeyArray = data.ReadCollection(data.ReadString16).ToArray();

                Client.SetRecoveryData(session, recoverySessionKeyArray);
            }

            InvokeEvent(result);
        }
    }
}
