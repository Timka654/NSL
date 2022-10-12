using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.TS.TranslateServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.TS.TranslateServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.TS.TranslateServerNetworkClient>;

namespace NSL.Node.BridgeServer.TS
{
    internal class TranslateServer
    {
        private const ushort SignSessionPID = 1;

        private const ushort SignSessionResultPID = SignSessionPID;

        public static int BindingPort => Program.Configuration.GetValue("translate.server.port", 6999);

        public static NetworkListener Listener { get; private set; }

        public static ILogger Logger { get; } = new PrefixableLoggerProxy(Program.Logger, "[TranslateServer]");

        public static void Run()
        {
            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>();

                    builder.AddPacketHandle(SignSessionPID, SignSessionReceiveHandle);
                })
                .WithBindingPoint($"http://*:{BindingPort}")
                .Build();

            Listener.Start();
        }

        private static void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
        }
    }
}
