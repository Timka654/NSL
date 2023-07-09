using NSL.Extensions.Session.Client.Packets;
using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Client
{
    public static class ClientExtensions
    {
        public static void AddNSLSessions<TClient>(this CoreOptions<TClient> options, Action<NSLSessionClientOptions> configure = null, string SOObjectKey = NSLSessionClientOptions.ObjectBagKey, string RPObjectKey = RequestProcessor.DefaultObjectBagKey)
            where TClient : BaseSocketNetworkClient
        {
            var o = new NSLSessionClientOptions();

            if(configure != null)
                configure(o);

            options.ObjectBag[SOObjectKey] = o;

            options.AddResponsePacketHandle(RecoverySessionPacket<TClient>.PacketId, client => client.GetRequestProcessor(RPObjectKey));
        }

        public static void SetNSLSessionInfo<TClient>(this IClient client, NSLSessionInfo info)
            where TClient : BaseSocketNetworkClient
        {
            var co = client.Options as ClientOptions<TClient>;

            var options = co.ObjectBag.Get<NSLSessionClientOptions>(NSLSessionClientOptions.ObjectBagKey, true);

            (client.GetUserData() as TClient).ObjectBag.Set(options.ClientSessionBagKey, info);

        }
    }
}
