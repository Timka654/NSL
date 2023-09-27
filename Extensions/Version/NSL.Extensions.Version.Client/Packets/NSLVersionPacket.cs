using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace NSL.Extensions.Version.Client.Packets
{
    public class NSLVersionPacket<T>
        where T : BaseSocketNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 3;

        private static NSLVersionInfo GetClientVersionInfo(T client)
        {
            client.ThrowIfObjectBagNull();

            return client.Network.Options.ObjectBag.Get<NSLVersionInfo>(NSLVersionInfo.ObjectBagKey, true);
        }

        private static OutputPacketBuffer FillPacket(OutputPacketBuffer packet, NSLVersionInfo versionInfo)
        {
            packet.PacketId = PacketId;

            if (!(packet is RequestPacketBuffer))
                packet.WriteGuid(Guid.Empty);

            versionInfo.WriteFullTo(packet);

            return packet;
        }

        public static void SendRequest(T client, Action<NSLVersionResult> onResponse, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            SendRequest(client, GetClientVersionInfo(client), onResponse, RPObjectKey);
        }

        public static void SendRequest(T client, NSLVersionInfo versionInfo, Action<NSLVersionResult> onResponse, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            SendRequest(client.GetRequestProcessor(RPObjectKey), versionInfo, onResponse);
        }

        public static void SendRequest(RequestProcessor processor, NSLVersionInfo versionInfo, Action<NSLVersionResult> onResponse)
        {
            var request = RequestPacketBuffer.Create();

            FillPacket(request, versionInfo);

            processor.SendRequest(request, data => { onResponse(NSLVersionResult.ReadFullFrom(data)); return true; });
        }

        public static async Task<NSLVersionResult> SendRequestAsync(T client, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            return await SendRequestAsync(client, GetClientVersionInfo(client), RPObjectKey);
        }

        public static async Task<NSLVersionResult> SendRequestAsync(T client, NSLVersionInfo versionInfo, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
        {
            client.ThrowIfObjectBagNull();

            return await SendRequestAsync(client.GetRequestProcessor(RPObjectKey), versionInfo);
        }

        public static async Task<NSLVersionResult> SendRequestAsync(RequestProcessor processor, NSLVersionInfo versionInfo)
        {
            var request = RequestPacketBuffer.Create();

            FillPacket(request, versionInfo);

            NSLVersionResult result = default;

            await processor.SendRequestAsync(request, data => { result = NSLVersionResult.ReadFullFrom(data); return Task.FromResult(true); });

            return result;
        }
    }
}
