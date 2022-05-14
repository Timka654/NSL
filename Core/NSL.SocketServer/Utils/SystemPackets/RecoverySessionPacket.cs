using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketServer.Utils
{
    public enum RecoverySessionResultEnum
    {
        Ok,
        NotFound
    }

    public delegate void OnRecoverySessionReceiveDelegate<T>(T client, string key, string[] keys);
}

namespace NSL.SocketServer.Utils.SystemPackets
{
    public class RecoverySessionPacket<T> : IPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 2;

        public event OnRecoverySessionReceiveDelegate<T> OnRecoverySessionReceiveEvent;

        public override void Receive(T client, InputPacketBuffer data)
        {
            string session = data.ReadString16();

            int count = data.ReadInt32();

            string[] keys = new string[count];

            for (int i = 0; i < count; i++)
            {
                keys[i] = data.ReadString16();
            }

            OnRecoverySessionReceiveEvent?.Invoke(client, session, keys);
        }

        public static void Send(IServerNetworkClient client, RecoverySessionResultEnum result, string session, string[] newKeys)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            packet.WriteByte((byte)result);

            if (result == RecoverySessionResultEnum.Ok)
            {
                packet.WriteString16(session);
                packet.WriteInt32(newKeys.Length);

                for (int i = 0; i < newKeys.Length; i++)
                {
                    packet.WriteString16(newKeys[i]);
                }
            }

            client.Send(packet);
        }
    }
}
