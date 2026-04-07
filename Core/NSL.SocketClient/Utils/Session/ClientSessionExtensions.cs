using NSL.SocketClient;
using NSL.SocketClient.Utils.Session.Packets;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using NSL.SocketCore.Network.Session;
using System;

namespace NSL.SocketClient.Utils.Session
{
    public static class ClientSessionExtensions
    {
        public static void AddNSLSessions<TClient>(this ClientOptions<TClient> options, Action<NSLSessionClientOptions> configure = null, string SOObjectKey = NSLSessionClientOptions.ObjectBagKey, string RPObjectKey = NSLObjectBagKeys.RequestProcessor)
            where TClient : BaseNetworkConnection
        {
            var o = new NSLSessionClientOptions();
            configure?.Invoke(o);
            options.ObjectBag[SOObjectKey] = o;
            options.AddResponsePacketHandle(NSLRecoverySessionPacket.PacketId, client => client.GetRequestProcessor(RPObjectKey));
        }

        public static NSLSessionInfo GetNSLSessionInfo<TClient>(this TClient client, string optionsObjectKey = NSLSessionClientOptions.ObjectBagKey)
            where TClient : BaseNetworkConnection
        {
            var options = client.GetNSLSessionOptions(optionsObjectKey);
            return client.GetNSLSessionInfo(options);
        }

        public static NSLSessionInfo GetNSLSessionInfo<TClient>(this TClient client, NSLSessionClientOptions options)
            where TClient : BaseNetworkConnection
        {
            return client.ObjectBag.Get<NSLSessionInfo>(options.ClientSessionBagKey);
        }

        public static NSLSessionClientOptions GetNSLSessionOptions<TClient>(this TClient client, string optionsObjectKey = NSLSessionClientOptions.ObjectBagKey)
            where TClient : BaseNetworkConnection
        {
            var co = client.Options as ClientOptions<TClient>;
            return co.ObjectBag.Get<NSLSessionClientOptions>(optionsObjectKey, true);
        }

        public static void SetNSLSessionInfo<TClient>(this TClient client, NSLSessionInfo info, string optionsObjectKey = NSLSessionClientOptions.ObjectBagKey)
            where TClient : BaseNetworkConnection
        {
            var options = client.GetNSLSessionOptions(optionsObjectKey);
            client.SetNSLSessionInfo(info, options);
        }

        public static void SetNSLSessionInfo<TClient>(this TClient client, NSLSessionInfo info, NSLSessionClientOptions options)
            where TClient : BaseNetworkConnection
        {
            client.ObjectBag.Set(options.ClientSessionBagKey, info);
        }
    }
}
