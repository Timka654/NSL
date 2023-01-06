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

        protected override void Receive(InputPacketBuffer data)
        {
            try
            { 
                var now = data.ReadDateTime();
                
                now = DateTime.UtcNow; // todo check logic
                
                var serverDT = data.ReadDateTime();

                now = now.AddMilliseconds(-(base.Client.Ping / 2));

                Client.LocalDateTime = now;
                Client.ServerDateTime = serverDT;

                Client.ServerDateTimeOffset = now - serverDT;
            }
            catch (Exception ex)
            {
                Options.RunException(ex);
            }
        }
    }
}
