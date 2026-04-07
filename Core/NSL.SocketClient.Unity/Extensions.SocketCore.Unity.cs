using NSL.SocketCore;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils;
using NSL.Utils.Unity;
using System;

namespace NSL.BuilderExtensions.SocketCore.Unity
{
    public static class Extensions
    {
        public static void AddConnectHandleForUnity(this IOptionableEndPointBuilder builder, Action<BaseNetworkConnection> handle)
        {
            builder.GetCoreOptions().OnClientConnectEvent += (client) => ThreadHelper.InvokeOnMain(() => handle(client));
        }

        public static void AddDisconnectHandleForUnity(this IOptionableEndPointBuilder builder, Action<BaseNetworkConnection> handle)
        {
            builder.GetCoreOptions().OnClientDisconnectEvent += (client) => ThreadHelper.InvokeOnMain(() => handle(client));
        }

        public static void AddExceptionHandleForUnity(this IOptionableEndPointBuilder builder, Action<Exception, BaseNetworkConnection> handle)
        {
            builder.GetCoreOptions().OnExceptionEvent += (ex, client) => ThreadHelper.InvokeOnMain(() => handle(ex, client));
        }

        public static void AddReceiveHandleForUnity<TClient>(this IHandleIOBuilder<TClient> builder, Action<BaseNetworkConnection, ushort, int> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.AddReceiveHandle((client, pid, len) => ThreadHelper.InvokeOnMain(() => handle(client, pid, len)));
        }

        public static void AddSendHandleForUnity<TClient>(this IHandleIOBuilder<TClient> builder, Action<BaseNetworkConnection, ushort, int, string> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.AddSendHandle((client, pid, len, stackTrace) => ThreadHelper.InvokeOnMain(() => handle(client, pid, len, stackTrace)));
        }
    }
}
