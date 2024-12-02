using NSL.BuilderExtensions.SocketCore;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.Node.RoomServer.Client.Data;
using System;
using System.Collections.Concurrent;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.EndPointBuilder;
using NSL.SocketServer.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer;
using NSL.Extensions.Session.Server;
using NSL.SocketCore.Utils.Logger;
using Microsoft.AspNetCore.Http;
using System.Linq;
using NSL.SocketCore.Extensions.Buffer;
using NSL.Utils;

namespace NSL.Node.RoomServer.Client
{
    public abstract partial class ClientServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected NodeRoomServerEntry Entry { get; }

        protected IBasicLogger Logger { get; }

        public ClientServerBaseEntry(NodeRoomServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[ClientServer]");
        }

        public abstract void Run();

        public async Task<RoomInfo> TryLoadRoomAsync(Guid roomId, Guid sessionId)
        {
            if (!roomMap.TryGetValue(sessionId, out var roomInfo))
            {
                var result = await Entry.ValidateSession(new BridgeServer.Shared.Requests.RoomSignSessionRequestModel()
                {
                    SessionIdentity = sessionId,
                    RoomIdentity = roomId
                });

                if (result.Result == true)
                {
                    roomInfo = roomMap.GetOrAdd(sessionId, id => new Lazy<Task<RoomInfo>>(async () =>
                    {
                        var room = new RoomInfo(Entry, sessionId, roomId);

                        room.OnRoomDisposed += () =>
                        {
                            roomMap.TryRemove(id, out _);
                            return Task.CompletedTask;
                        };

                        await room.SetStartupInfo(new NodeRoomStartupInfo(result.Options));

                        return room;
                    }, true));
                }
            }

            if (roomInfo == null)
                return null;

            return await roomInfo.Value;
        }

        protected NSLSessionManager<TransportNetworkClient> sessionManager;

        protected TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<TransportNetworkClient>, IHandleIOBuilder<TransportNetworkClient>
        {
            builder.AddConnectHandle(client => client.InitializeObjectBag());

            var options = builder.GetCoreOptions() as ServerOptions<TransportNetworkClient>;

            options.SetDefaultResponsePID();

            if (Entry.ReconnectSessionLifeTime.HasValue)
            {
                sessionManager = options.AddNSLSessions(c =>
                {
                    c.CloseSessionDelay = Entry.ReconnectSessionLifeTime.Value;

                    c.OnClientValidate += (client) =>
                    {
                        if (client.Room == null || client.ManualDisconnected || client.Node == null)
                            return Task.FromResult(false);

                        bool result = client.Room?.ValidateSession(client.Node) ?? false;

                        return Task.FromResult(result);
                    };

                    c.OnRecoverySession += async (client, session) =>
                    {
                        var sSession = session as NSLServerSessionInfo<TransportNetworkClient>;

                        options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Try recovery session {sSession.Session} from {client.Network?.GetRemotePoint()}");

                        //client.Network.ChangeUserData(sSession.Client);

                        var room = client.Room;

                        if (room == null)
                        {
                            Task.Delay(1000).ContinueWith((t) =>
                            {
                                sessionManager.RemoveSession(client);

                                client?.Disconnect();
                            }).RunAsync();

                            return;
                        }

                        await room.RecoverySession(client.Node);

                    };

                    c.OnExpiredSession += (network, session) =>
                    {
                        var sSession = session as NSLServerSessionInfo<TransportNetworkClient>;

                        if (sSession != null)
                        {
                            options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Session expired {sSession.Session}");
                        }

                        network.Room?.DisconnectNode(network);

                        return Task.CompletedTask;
                    };
                });
            }

            builder.SetLogger(Logger);

            builder.AddAsyncPacketHandle(
                RoomPacketEnum.SignSessionRequest, SignInPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.TransportMessage, TransportPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.BroadcastMessage, BroadcastPacketHandle);
            //builder.AddAsyncPacketHandle(
            //    RoomPacketEnum.ReadyNodeRequest, ReadyPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.ExecuteMessage, ExecutePacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.DisconnectMessage, DisconnectMessagePacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.NodeChangeEndPointMessage, ChangeConnectionPointPacketHandle);

            builder.AddDisconnectHandle(client =>
            {
                var room = client.Room;
                if (room != null)
                {
                    if (Entry.ReconnectSessionLifeTime.HasValue)
                        room.OnClientDisconnected(client);
                    else
                        room.DisconnectNode(client);
                }
            });

            return builder;
        }

        private ConcurrentDictionary<Guid, Lazy<Task<RoomInfo>>> roomMap = new();
    }
}
