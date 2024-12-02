using NSL.Node.Core.Models.Requests;
using NSL.Node.Core.Models.Response;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Client
{
    public abstract partial class ClientServerBaseEntry
    {
        private void BroadcastPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.Data;

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(RoomPacketEnum.TransportMessage);

            pbuf.WriteString(client.Id);

            pbuf.Write(body);

            client.Room.Broadcast(pbuf);
        }

        void ChangeConnectionPointPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
            => client.Room?.ChangeNodeConnectionPoint(client, buffer.ReadString());

        void DisconnectMessagePacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
            => client.Disconnect();

        void ExecutePacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
            => client.Room?.Execute(client, buffer);

        void TransportPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
            => client.Room?.Transport(client, buffer);

        TimeSpan signInPerfMax = TimeSpan.Zero;

        async Task SignInPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var response = buffer.CreateResponse();

            var request = RoomNodeSignInRequestModel.ReadFullFrom(buffer);

            var result = new RoomNodeSignInResponseModel();

            client.EndPoint = request.ConnectionEndPoint;

            Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"RoomId {request.RoomId}, Token {request.Token} connect");

            var test = Stopwatch.StartNew();

            var roomInfo = await TryLoadRoomAsync(request.RoomId, request.SessionId);

            if (roomInfo != null)
            {
                var splitedToken = request.Token.Split(':');

                var nodeId = splitedToken.First();

                var validatePlayer = await Entry.ValidateSessionPlayer(new BridgeServer.Shared.Requests.RoomSignSessionPlayerRequestModel()
                {
                    SessionId = request.SessionId,
                    PlayerId = nodeId
                });

                if (!validatePlayer.ExistsSession)
                {
                    Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"RoomId {request.RoomId}, Token {request.Token} - not found session!!");

                    if (roomMap.TryRemove(request.SessionId, out var expiredSession))
                    {
                        client.Network?.Options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $" - [SignIn] Remove session {request.SessionId} by no exists information on Bridge");
                        expiredSession.Value.Dispose();
                    }
                }

                if (validatePlayer.ExistsPlayer)
                {
                    client.Room = roomInfo;
                    client.Id = client.NodeId = nodeId;
                    client.Token = string.Join(':', splitedToken.Skip(1).ToArray());


                    result.Success = await client.Room.AddClient(client);


                    if (result.Success)
                    {
                        result.NodeId = client.NodeId;
                        result.Options = roomInfo.GetClientOptions();
                        var session = sessionManager?.CreateSession(client, client.NodeId.ToString());
                        result.SessionInfo = session;
                    }
                    else
                        Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"RoomId {request.RoomId}, Token {request.Token} - cannot add to room");
                }
                else
                    Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"RoomId {request.RoomId}, Token {request.Token} - not exists player!!");
            }
            else
                Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"RoomId {request.RoomId}, Token {request.Token} - room info not found!!");

            if (signInPerfMax < test.Elapsed)
            {
                signInPerfMax = test.Elapsed;
                Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"perfMax {signInPerfMax.TotalMilliseconds}ms");
            }
            result.WriteFullTo(response);

            client.Network.Send(response);
        }
    }
}
