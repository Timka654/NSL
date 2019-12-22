using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketCore.Utils.Buffer;

namespace SCL.Utils
{
    public class IPacket<TClient> where TClient : BaseSocketNetworkClient
    {
        public IPacket(ClientOptions<TClient> options)
        {
            Options = options;
        }

        protected ClientOptions<TClient> Options { get; private set; }

        public bool SuccessSend { get; protected set; }

        protected TClient Client => Options.ClientData;

        protected virtual void Receive(InputPacketBuffer data)
        {
        }

        public ClientOptions<TClient>.PacketHandle GetReceiveHandle()
        {
            return Receive;
        }

        protected void Send(OutputPacketBuffer packet)
        {
            if (Client.Network?.GetState() == true)
            {
                Client.Network.Send(packet);
                SuccessSend = true;
                return;
            }

            Options.RunExtension(new Exception("Не возможно отправить сообщение, соединение не установлено"));
            SuccessSend = false;
        }

        protected void Send<O>(ushort packetId, O obj, string scheme)
        {
            if (Client.Network?.GetState() == true)
            {
                Client.Network.SendSerialize<O>(packetId, obj, scheme);
                SuccessSend = true;
                return;
            }

            Options.RunExtension(new Exception("Не возможно отправить сообщение, соединение не установлено"));
            SuccessSend = false;
        }
    }
}
