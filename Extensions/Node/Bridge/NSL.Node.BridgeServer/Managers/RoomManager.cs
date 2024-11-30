using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.SocketCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Managers
{
    public class RoomManager
    {
        public delegate Task<bool> SignInServerValidatorDelegate(RoomServerNetworkClient roomServer, RoomSignInRequestModel request);
        public delegate Task ServerSignedHandlerDelegate(RoomServerNetworkClient roomServer, RoomSignInRequestModel request);
        public delegate Task<IEnumerable<RoomServerNetworkClient>> ServerMissedHandlerDelegate(LobbyCreateRoomSessionRequestModel request, MissedServerReasonEnum reason);

        public delegate Task ServerConnectionLostHandlerDelegate(RoomServerNetworkClient roomServer);

        public SignInServerValidatorDelegate SignInServerValidator { get; set; }
            = (instance, request) => Task.FromResult(true);

        public ServerSignedHandlerDelegate ServerSignedHandler { get; set; }
            = (instance, request) => Task.CompletedTask;

        public ServerMissedHandlerDelegate ServerMissedHandler { get; set; }
            = (request, reason) => Task.FromResult(Enumerable.Empty<RoomServerNetworkClient>());

        public ServerConnectionLostHandlerDelegate ServerConnectionLostHandler { get; set; }
            = (instance) => Task.CompletedTask;

        public ServerConnectionLostHandlerDelegate ServerDisconnectedHandler { get; set; }
            = (instance) => Task.CompletedTask;

        public async void OnDisconnectedRoomServer(RoomServerNetworkClient client)
        {
            if (!client.Signed)
                return;

            await ServerConnectionLostHandler(client);

            await Task.Delay(delayMSAfterDisconnectRoomServer);

            locker.WaitOne();
            try
            {
                if (connectedServers[client.Id] != client || client.Network?.GetState() == true)
                    return;

                connectedServers.TryRemove(client.Id, out _);

                client.Disconnect();

                await ServerDisconnectedHandler(client);

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                locker.Set();
            }
        }

        private AutoResetEvent locker = new AutoResetEvent(true);

        public async Task<bool> TryRoomServerConnect(RoomServerNetworkClient client, RoomSignInRequestModel request)
        {
            if (!await SignInServerValidator(client, request))
                return false;

            locker.WaitOne();

            try
            {
                if (Guid.Empty.Equals(request.Identity))
                    request.Identity = Guid.NewGuid();
                else
                {
                    if (connectedServers.TryGetValue(request.Identity, out var exists))
                    {
                        if (exists.GetState())
                            return false;

                        SignRoom(client, request);

                        connectedServers[request.Identity] = client;

                        client.ChangeOwner(exists);

                        await ServerSignedHandler(client, request);

                        locker.Set();

                        return true;
                    }
                }

                while (!connectedServers.TryAdd(request.Identity, client))
                {
                    request.Identity = Guid.NewGuid();
                }

                SignRoom(client, request);

                await ServerSignedHandler(client, request);

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                locker.Set();
            }

            return true;
        }

        private void SignRoom(RoomServerNetworkClient client, RoomSignInRequestModel request)
        {
            client.Id = request.Identity;

            client.ConnectionEndPoint = request.ConnectionEndPoint;

            client.Signed = true;
        }

        public async Task<CreateRoomSessionResponseModel> CreateRoomSession(LobbyServerNetworkClient client, LobbyCreateRoomSessionRequestModel request)
        {
            var result = new CreateRoomSessionResponseModel();


            locker.WaitOne();

            try
            {
                IEnumerable<RoomServerNetworkClient> servers = default;

                if (request.SpecialServer.HasValue)
                {
                    if (connectedServers.TryGetValue(request.SpecialServer.Value, out var server))
                        servers = Enumerable.Repeat(server, 1);
                    else
                        servers = await ServerMissedHandler(request, MissedServerReasonEnum.CannotFoundSpecialInstance);
                }
                else
                {
                    IEnumerable<RoomServerNetworkClient> serverSelector = connectedServers.Values;

                    if (request.Location != default)
                    {
                        serverSelector = serverSelector.Where(x => request.Location.Equals(x.Location));

                        if (!serverSelector.Any())
                            serverSelector = await ServerMissedHandler(request, MissedServerReasonEnum.CannotFoundLocationInstance);
                    }

                    servers = serverSelector.OrderBy(x => x.SessionsCount);
                }

                if (request.InstanceWeight.HasValue)
                {
                    servers = servers.Where(x => x.MaxWeight.HasValue).Where(x => x.MaxWeight - x.Weight > request.InstanceWeight);

                    if (!servers.Any())
                        servers = await ServerMissedHandler(request, MissedServerReasonEnum.CannotFoundInstanceWeight);
                }

                servers = servers.Where(x => x.Network?.GetState() == true).Take(request.NeedPointCount);

                if (!servers.Any())
                    servers = await ServerMissedHandler(request, MissedServerReasonEnum.UnavailableInstances);

                result.Result = servers.Any();

                if (result.Result)
                {
                    var sessions = new List<RoomSession>();

                    result.CreateTime = DateTime.UtcNow;

                    result.ConnectionPoints = servers.Select(server =>
                    {
                        if (request.InstanceWeight.HasValue)
                            server.Weight += request.InstanceWeight.Value;

                        var session = server.CreateSession(new RoomSession(client, server, request));

                        session.OnDestroy += session =>
                        {
                            sessions.Remove(session);

                            if (sessions.Any() == false)
                                client.Rooms.Remove(session.RoomIdentity, out _);
                        };

                        sessions.Add(session);

                        return new Shared.RoomServerPointInfo()
                        {
                            Endpoint = server.ConnectionEndPoint,
                            SessionId = session.SessionId
                        };
                    }).ToList();

                    client.Rooms.AddOrUpdate(request.RoomId, k => sessions, (k, o) => sessions);
                }

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                locker.Set();
            }

            return result;
        }

        private ConcurrentDictionary<Guid, RoomServerNetworkClient> connectedServers = new ConcurrentDictionary<Guid, RoomServerNetworkClient>();

        private int delayMSAfterDisconnectRoomServer = 10_000;

        #region Builder

        public RoomManager WithSignInServerValidator(SignInServerValidatorDelegate value)
        {
            SignInServerValidator = value;

            return this;
        }

        public RoomManager WithServerSignedHandler(ServerSignedHandlerDelegate value)
        {
            ServerSignedHandler = value;

            return this;
        }

        public RoomManager WithServerMissedHandler(ServerMissedHandlerDelegate value)
        {
            ServerMissedHandler = value;

            return this;
        }

        public RoomManager WithServerConnectionLostHandler(ServerConnectionLostHandlerDelegate value)
        {
            ServerConnectionLostHandler = value;

            return this;
        }

        public RoomManager WithServerDisconnectedHandler(ServerConnectionLostHandlerDelegate value)
        {
            ServerDisconnectedHandler = value;

            return this;
        }

        public RoomManager WithWaitServerRecoverySession(int delay)
        {
            delayMSAfterDisconnectRoomServer = delay;
            return this;
        }

        #endregion
    }

    public record CreateSignResult(string endPoint, Guid id);

    public enum MissedServerReasonEnum
    {
        CannotFoundSpecialInstance,
        CannotFoundInstanceWeight,
        CannotFoundInstanceCount,
        CannotFoundLocationInstance,
        UnavailableInstances

    }
}
