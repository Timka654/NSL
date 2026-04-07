using System;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketClient.Utils.SystemPackets
{
    public class ClientSystemTimePacket
    {
        public const ushort PacketId = (ushort)NSLSystemPacketEnum.SystemTime;

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

    public class ClientSystemTimePacket<T> : IClientPacket<T> where T : BaseNetworkConnection, new()
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

                var client = base.Client;
                client.InitializeObjectBag();
                client.ObjectBag.Set(NSLObjectBagKeys.LocalDateTime, now);
                client.ObjectBag.Set(NSLObjectBagKeys.ServerDateTime, serverDT);
                client.ObjectBag.Set(NSLObjectBagKeys.ServerTimeOffset, now - serverDT);
            }
            catch (Exception ex)
            {
                Options.RunException(ex);
            }
        }
    }

    /// <summary>
    /// Extension methods for accessing server time data stored in <see cref="BaseNetworkConnection.ObjectBag"/>.
    /// </summary>
    public static class ServerTimeExtensions
    {
        public static TimeSpan GetServerDateTimeOffset(this BaseNetworkConnection client)
            => client.ObjectBag?.Get<TimeSpan>(NSLObjectBagKeys.ServerTimeOffset) ?? TimeSpan.Zero;

        public static DateTime GetServerDateTime(this BaseNetworkConnection client)
            => client.ObjectBag?.Get<DateTime>(NSLObjectBagKeys.ServerDateTime) ?? default;

        public static DateTime GetLocalDateTime(this BaseNetworkConnection client)
            => client.ObjectBag?.Get<DateTime>(NSLObjectBagKeys.LocalDateTime) ?? default;

        public static DateTime GetClientDateTime(this BaseNetworkConnection client, DateTime serverDateTime)
            => serverDateTime + client.GetServerDateTimeOffset();

        public static DateTime? GetClientDateTime(this BaseNetworkConnection client, DateTime? serverDateTime)
            => serverDateTime.HasValue ? client.GetClientDateTime(serverDateTime.Value) : null;

        public static void RequestServerTimeOffset(this BaseNetworkConnection client)
            => ClientSystemTimePacket.SendRequest(client.Network);
    }
}
