using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using System;
using System.Threading;

namespace SocketClient.Utils.SystemPackets
{
    public class ClientAliveConnectionPacket<T> : IClientPacket<T> where T: BaseSocketNetworkClient
    {
        protected override void Receive(InputPacketBuffer data)
        {
            Client.PongProcess();
        }

        public ClientAliveConnectionPacket(ClientOptions<T> options) : base(options)
        {
        }
    }
}
