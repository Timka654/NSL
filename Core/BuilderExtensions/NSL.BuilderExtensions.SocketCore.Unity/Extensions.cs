using NSL.SocketClient.Utils;
using NSL.SocketClient;
using NSL.SocketCore;
using System;
using NSL.EndPointBuilder;
using System.Reflection;
using NSL.SocketCore.Utils;
using NSL.Utils.Unity;

namespace NSL.BuilderExtensions.SocketCore.Unity
{
    public static class Extensions
    {
        public static void AddConnectHandleForUnity<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ClientConnect handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientConnectEvent += (client) => ThreadHelper.InvokeOnMain(() => handle(client));
        }

        public static void AddDisconnectHandleForUnity<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ClientDisconnect handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnClientDisconnectEvent += (client) => ThreadHelper.InvokeOnMain(() => handle(client));
        }

        public static void AddExceptionHandleForUnity<TClient>(this IOptionableEndPointBuilder<TClient> builder, CoreOptions<TClient>.ExceptionHandle handle)
            where TClient : INetworkClient, new()
        {
            builder.GetCoreOptions().OnExceptionEvent += (ex, client) => ThreadHelper.InvokeOnMain(() => handle(ex, client));
        }

        public static void AddReceiveHandleForUnity<TClient>(this IHandleIOBuilder<TClient> builder, ReceivePacketDebugInfo<TClient> handle)
            where TClient : IClient
        {
            builder.AddReceiveHandle((client, pid, len) => ThreadHelper.InvokeOnMain(() => handle(client, pid, len)));
        }

        public static void AddSendHandleForUnity<TClient>(this IHandleIOBuilder<TClient> builder, SendPacketDebugInfo<TClient> handle)
            where TClient : IClient
        {
            {
                builder.AddSendHandle((client, pid, len, stackTrace) => ThreadHelper.InvokeOnMain(() => handle(client, pid, len, stackTrace)));
            }
        }
    }
}
