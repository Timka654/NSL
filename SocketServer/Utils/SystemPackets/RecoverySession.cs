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

    public delegate void OnRecoverySessionReceiveDelegate<T>(T client,long id, string[] keys);
}

namespace SocketServer.Utils.SystemPackets
{
    public class RecoverySession<T> :IPacket<T> where T: INetworkClient
    {
        public static RecoverySession<T> Instance { get; private set; }

        public event OnRecoverySessionReceiveDelegate<T> OnRecoverySessionReceiveEvent;

        public RecoverySession()
        {
            Instance = this;
        }

        public void Receive(T client, InputPacketBuffer data)
        {
            long id = data.ReadInt64();

            int count = data.ReadInt32();

            string[] keys = new string[count];

            for (int i = 0; i < count; i++)
            {
                keys[i] = data.ReadString16();
            }

            OnRecoverySessionReceiveEvent?.Invoke(client, id, keys);
        }

        public static void Send(INetworkClient client, RecoverySessionResultEnum result, long? id, string[] newKeys)
        {
               var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)Enums.ServerPacketEnum.RecoverySessionResult
            };

            packet.WriteByte((byte)result);

            if (id.HasValue)
            {
                packet.WriteInt64(id.Value);
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
