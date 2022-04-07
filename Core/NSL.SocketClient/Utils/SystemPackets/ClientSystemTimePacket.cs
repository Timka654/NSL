using System;
using SocketCore.Utils.Buffer;

namespace SocketClient.Utils.SystemPackets
{
    internal interface IClientSystemTimePacket
    {
        void Mark();
    }

    public class ClientSystemTimePacket<T> : IClientPacket<T>, IClientSystemTimePacket where T : BaseSocketNetworkClient
    {
        public ClientSystemTimePacket(ClientOptions<T> options) : base(options)
        {
        }

        private DateTime mark;

        public void Mark()
        {
            mark = DateTime.UtcNow;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            try
            {
                var now = DateTime.UtcNow;

                var dt = data.ReadDateTime().AddMilliseconds(((now - mark).TotalMilliseconds / 2));

                Client.ServerDateTimeOffset = now - dt;
            }
            catch (Exception ex)
            {
                Options.RunException(ex);
            }
        }

    }
}
