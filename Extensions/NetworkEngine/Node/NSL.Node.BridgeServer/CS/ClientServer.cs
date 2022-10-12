using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Buffer;

using NetworkClient = NSL.Node.BridgeServer.CS.ClientServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.CS.ClientServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.CS.ClientServerNetworkClient>;

namespace NSL.Node.BridgeServer.CS
{
    internal class ClientServer
    {
        private const ushort SignSessionPID = 1;

        private const ushort SignSessionResultPID = SignSessionPID;

        public static int BindingPort => Program.Configuration.GetValue("client.server.port", 7000);

        public static NetworkListener Listener { get; private set; }

        public static ILogger Logger { get; } = new PrefixableLoggerProxy(Program.Logger, "[ClientServer]");

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

        #region Handles

        private static void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            client.ServerIdentity = data.ReadString16();
            client.SessionIdentity = data.ReadString16();
        }

        #endregion
    }
}
