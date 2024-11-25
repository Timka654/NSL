using NSL.Extensions.Session.Client.Packets;
using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using System;

namespace NSL.Extensions.Session.Client
{
    public static class ClientExtensions
    {
        public static void AddNSLSessions<TClient>(this ClientOptions<TClient> options, Action<NSLSessionClientOptions> configure = null, string SOObjectKey = NSLSessionClientOptions.ObjectBagKey, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            var o = new NSLSessionClientOptions();

            if (configure != null)
                configure(o);

            options.ObjectBag[SOObjectKey] = o;

            options.AddResponsePacketHandle(NSLRecoverySessionPacket.PacketId, client => client.GetRequestProcessor(RPObjectKey));
        }

        public static NSLSessionInfo GetNSLSessionInfo<TClient>(this TClient client, string optionsObjectKey = NSLSessionClientOptions.ObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            var options = client.GetNSLSessionOptions(optionsObjectKey);

            return client.GetNSLSessionInfo(options);
        }

        public static NSLSessionInfo GetNSLSessionInfo<TClient>(this TClient client, NSLSessionClientOptions options)
            where TClient : BaseSocketNetworkClient
        {
            return client.ObjectBag.Get<NSLSessionInfo>(options.ClientSessionBagKey);
        }

        public static NSLSessionClientOptions GetNSLSessionOptions<TClient>(this TClient client, string optionsObjectKey = NSLSessionClientOptions.ObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            var co = client.ClientOptions as ClientOptions<TClient>;

            return co.ObjectBag.Get<NSLSessionClientOptions>(optionsObjectKey, true);
        }

        public static void SetNSLSessionInfo<TClient>(this TClient client, NSLSessionInfo info, string optionsObjectKey = NSLSessionClientOptions.ObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            var options = client.GetNSLSessionOptions(optionsObjectKey);

            client.SetNSLSessionInfo(info, options);
        }

        public static void SetNSLSessionInfo<TClient>(this TClient client, NSLSessionInfo info, NSLSessionClientOptions options)
            where TClient : BaseSocketNetworkClient
        {
            client.ObjectBag.Set(options.ClientSessionBagKey, info);
        }
    }
}
