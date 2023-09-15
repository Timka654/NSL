using NSL.Extensions.Session;
using NSL.Extensions.Session.Client;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Client.Packets
{
    public class NSLRecoverySessionPacket<T>
        where T : BaseSocketNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 2;

        public static void Send(T client)
        {
            var session = GetClientSessionInfo(client);

            var packet = FillPacket(new OutputPacketBuffer(), session);

            client.Network.Send(packet);
        }

        public static OutputPacketBuffer BuildPacket(T client)
        {
            return FillPacket(new OutputPacketBuffer(), client);
        }

        private static NSLSessionInfo GetClientSessionInfo(T client)
        {
            client.ThrowIfObjectBagNull();

            var options = (client.ClientOptions as ClientOptions<T>).ObjectBag.Get<NSLSessionClientOptions>(NSLSessionClientOptions.ObjectBagKey, true);

            return client.ObjectBag.Get<NSLSessionInfo>(options.ClientSessionBagKey, true);
        }

        private static OutputPacketBuffer FillPacket(OutputPacketBuffer packet, T client)
        {
            return FillPacket(packet, GetClientSessionInfo(client));
        }

        public static OutputPacketBuffer BuildPacket(NSLSessionInfo sessionInfo)
        {
            return FillPacket(new OutputPacketBuffer(), sessionInfo);
        }

        private static OutputPacketBuffer FillPacket(OutputPacketBuffer packet, NSLSessionInfo sessionInfo)
        {
            packet.PacketId = PacketId;

            if (!(packet is RequestPacketBuffer))
                packet.WriteGuid(Guid.Empty);

            sessionInfo.WriteFullTo(packet);

            return packet;
        }

        public static void SendRequest(T client, Action<NSLRecoverySessionResult> onResponse, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            SendRequest(client, GetClientSessionInfo(client), onResponse, RPObjectKey);
        }

        public static void SendRequest(T client, NSLSessionInfo sessionInfo, Action<NSLRecoverySessionResult> onResponse, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            SendRequest(client.GetRequestProcessor(RPObjectKey), sessionInfo, onResponse);
        }

        public static void SendRequest(RequestProcessor processor, NSLSessionInfo sessionInfo, Action<NSLRecoverySessionResult> onResponse)
        {
            var request = RequestPacketBuffer.Create();

            FillPacket(request, sessionInfo);

            processor.SendRequest(request, data => { onResponse(NSLRecoverySessionResult.ReadFullFrom(data)); return true; });
        }

        public static async Task<NSLRecoverySessionResult> SendRequestAsync(T client, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            return await SendRequestAsync(client, GetClientSessionInfo(client), RPObjectKey);
        }

        public static async Task<NSLRecoverySessionResult> SendRequestAsync(T client, NSLSessionInfo sessionInfo, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            return await SendRequestAsync(client.GetRequestProcessor(RPObjectKey), sessionInfo);
        }

        public static async Task<NSLRecoverySessionResult> SendRequestAsync(RequestProcessor processor, NSLSessionInfo sessionInfo)
        {
            var request = RequestPacketBuffer.Create();

            FillPacket(request, sessionInfo);

            NSLRecoverySessionResult result = default;

            await processor.SendRequestAsync(request, data => { result = NSLRecoverySessionResult.ReadFullFrom(data); return Task.CompletedTask; });

            return result;
        }
    }
}
