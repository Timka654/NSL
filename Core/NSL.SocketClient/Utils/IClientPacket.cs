using System;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace NSL.SocketClient.Utils
{
    public class IClientPacket<TClient> : IPacket<TClient> where TClient : BaseSocketNetworkClient
    {
        public IClientPacket(ClientOptions<TClient> options)
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
            if (Client?.Network?.GetState() == true)
            {
                Client.Network.Send(packet);
                SuccessSend = true;
                return;
            }

            Options.RunException(new Exception("Не возможно отправить сообщение, соединение не установлено"));
            SuccessSend = false;
        }

        //protected void Send<O>(ushort packetId, O obj, string scheme)
        //{
        //    if (Client.Network?.GetState() == true)
        //    {
        //        Client.Network.SendSerialize<O>(packetId, obj, scheme);
        //        SuccessSend = true;
        //        return;
        //    }

        //    Options.RunException(new Exception("Не возможно отправить сообщение, соединение не установлено"));
        //    SuccessSend = false;
        //}

        public override void Receive(TClient client, InputPacketBuffer data)
        {
            Receive(data);
        }
    }
}
