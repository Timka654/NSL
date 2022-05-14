using System;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketClient.Utils.SystemPackets
{
    public class ClientSystemTimePacket<T> : IClientPacket<T> where T : BaseSocketNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 1;

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

        public static void SendRequest(T client)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = PacketId
            };

            packet.WriteDateTime(DateTime.UtcNow);

            client.Network.Send(packet);
        }
    }
}
