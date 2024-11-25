using NSL.Extensions.Version.Client.Packets;
using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using System;

namespace NSL.Extensions.Version.Client
{
    public static class ClientExtensions
    {
        public static void AddNSLVersion<TClient>(this CoreOptions<TClient> options, Action<NSLVersionInfo> configure = null, string SOObjectKey = NSLVersionInfo.ObjectBagKey, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            var o = new NSLVersionInfo();

            if (configure != null)
                configure(o);

            options.ObjectBag[SOObjectKey] = o;

            options.AddResponsePacketHandle(NSLVersionPacket<TClient>.PacketId, client => client.GetRequestProcessor(RPObjectKey));
        }
    }
}
