using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.LS
{
    internal class LobbyServer
    {
        private const ushort SignServerPID = 1;

        private const ushort SignServerResultPID = SignServerPID;

        private const ushort ValidateSessionPID = 2;
        private const ushort ValidateSessionResultPID = ValidateSessionPID;

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

                    builder.AddPacketHandle(SignServerPID, SignServerReceiveHandle);

                    builder.AddPacketHandle(ValidateSessionResultPID, ValidateSessionReceiveHandle);
                })
                .WithBindingPoint($"http://*:{BindingPort}")
                .Build();

            Listener.Start();
        }

        private static void SignServerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            client.Identity = data.ReadString16();

            var identityKey = data.ReadString16();

            //todo:check identity key
            bool result = true;

            if (result)
            {
                connectedServers.Remove(client.Identity, out _);

                connectedServers.TryAdd(client.Identity, client);
            }

            var packet = new OutputPacketBuffer()
            {
                PacketId = SignServerResultPID
            };

            packet.WriteBool(result);

            client.Network.Send(packet);
        }

        private static void ValidateSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            client.ValidateRequestBuffer.ProcessWaitResponse(data);
        }

        public static async Task<bool> ValidateSession(string serverIdentity, string session)
        {
            if (!connectedServers.TryGetValue(serverIdentity, out var server))
                return false;

            bool result = default;

            await server.ValidateRequestBuffer.CreateWaitRequest(packet =>
            {
                packet.PacketId = ValidateSessionPID;

                packet.WriteString16(session);
            }, data =>
            {
                if (data != default)
                    result = data.ReadBool();

                return Task.CompletedTask;
            });

            return result;
        }

        private static ConcurrentDictionary<string, NetworkClient> connectedServers = new ConcurrentDictionary<string, NetworkClient>();
    }
}
