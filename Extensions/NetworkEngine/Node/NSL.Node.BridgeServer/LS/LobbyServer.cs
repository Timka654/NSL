using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;

namespace NSL.Node.BridgeServer.LS
{
    internal class LobbyServer
    {
        private const ushort SignSessionPID = 1;

        private const ushort SignSessionResultPID = SignSessionPID;

        public static int BindingPort => Program.Configuration.GetValue("lobby.server.port", 6999);

        public static NetworkListener Listener { get; private set; }

        public static ILogger Logger { get; } = new PrefixableLoggerProxy(Program.Logger, "[LobbyServer]");

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
