using System;
using NSL.SocketCore;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketClient.Utils.SystemPackets
{
    public class ClientSystemTimePacket
    {
        public const ushort PacketId = ushort.MaxValue - 1;

        public static void SendRequest(IClient client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            packet.WriteDateTime(DateTime.UtcNow);

            client.Send(packet);
        }
    }

    public class ClientSystemTimePacket<T> : IClientPacket<T> where T : BaseSocketNetworkClient
    {
        public ClientSystemTimePacket(ClientOptions<T> options) : base(options)
        {
        }

        private DateTime mark;

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
