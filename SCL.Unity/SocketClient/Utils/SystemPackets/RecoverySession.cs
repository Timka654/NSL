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
        NotFound,
        NoData
    }
}

namespace SCL.SocketClient.Utils.SystemPackets
{
    public class RecoverySession<T> : IPacketMessage<T, RecoverySessionResultEnum> where T : BaseSocketNetworkClient
    {
        internal static RecoverySession<T> Instance;
        public RecoverySession(ClientOptions<T> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            var result = (RecoverySessionResultEnum)data.ReadByte();
            
            if (result == RecoverySessionResultEnum.Ok)
            {
                Client.Session = data.ReadString16();
                Client.RecoverySessionKeyArray = new string[data.ReadInt32()];
                for (int i = 0; i < Client.RecoverySessionKeyArray.Count(); i++)
                {
                    Client.RecoverySessionKeyArray[i] = data.ReadString16();
                }
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
                PacketId = (ushort)Enums.ClientPacketEnum.RecoverySession
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
