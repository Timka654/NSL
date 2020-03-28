using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;

namespace SocketServer.Utils
{
    public enum RecoverySessionResultEnum
    {
        Ok,
        NotFound
    }

    public delegate void OnRecoverySessionReceiveDelegate<T>(T client, string key, string[] keys);
}

namespace SocketServer.Utils.SystemPackets
{
    public class RecoverySession<T> : IPacket<T> where T : IServerNetworkClient
    {
        public static RecoverySession<T> Instance { get; private set; }

        public event OnRecoverySessionReceiveDelegate<T> OnRecoverySessionReceiveEvent;

        public RecoverySession()
        {
            Instance = this;
        }

        public void Receive(T client, InputPacketBuffer data)
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
                PacketId = (ushort)ServerPacketEnum.RecoverySessionResult
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
