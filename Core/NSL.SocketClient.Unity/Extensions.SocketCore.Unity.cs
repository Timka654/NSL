using NSL.SocketCore;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils;
using NSL.Utils.Unity;

namespace NSL.BuilderExtensions.SocketCore.Unity
{
    public static class Extensions
    {
        public static void AddConnectHandleForUnity<TClient>(this IOptionableEndPointBuilder<TClient> builder, Action<TClient> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.GetCoreOptions().OnClientConnectEvent += (client) => ThreadHelper.InvokeOnMain(() => handle((TClient)client));
        }

        public static void AddDisconnectHandleForUnity<TClient>(this IOptionableEndPointBuilder<TClient> builder, Action<TClient> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.GetCoreOptions().OnClientDisconnectEvent += (client) => ThreadHelper.InvokeOnMain(() => handle((TClient)client));
        }

        public static void AddExceptionHandleForUnity<TClient>(this IOptionableEndPointBuilder<TClient> builder, Action<Exception, TClient> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.GetCoreOptions().OnExceptionEvent += (ex, client) => ThreadHelper.InvokeOnMain(() => handle(ex, (TClient)client));
        }

        public static void AddReceiveHandleForUnity<TClient>(this IHandleIOBuilder<TClient> builder, Action<TClient, ushort, int> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.AddReceiveHandle((client, pid, len) => ThreadHelper.InvokeOnMain(() => handle((TClient)client, pid, len)));
        }

        public static void AddSendHandleForUnity<TClient>(this IHandleIOBuilder<TClient> builder, Action<TClient, ushort, int, string> handle)
            where TClient : BaseNetworkConnection, new()
        {
            builder.AddSendHandle((client, pid, len, stackTrace) => ThreadHelper.InvokeOnMain(() => handle((TClient)client, pid, len, stackTrace)));
        }
    }
}
