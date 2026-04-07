using NSL.SocketClient;
using NSL.SocketClient.Utils.Version.Packets;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using NSL.SocketCore.Network.Version;
using System;

namespace NSL.SocketClient.Utils.Version
{
    public static class ClientVersionExtensions
    {
        public static void AddNSLVersion<TClient>(this CoreOptions<TClient> options, Action<NSLVersionInfo> configure = null, string SOObjectKey = NSLVersionInfo.ObjectBagKey, string RPObjectKey = NSLObjectBagKeys.RequestProcessor)
            where TClient : BaseNetworkConnection
        {
            var info = new NSLVersionInfo();
            configure?.Invoke(info);
            options.ObjectBag[SOObjectKey] = info;
            options.AddResponsePacketHandle(NSLVersionPacket<TClient>.PacketId, client => client.GetRequestProcessor(RPObjectKey));
        }
    }
}
