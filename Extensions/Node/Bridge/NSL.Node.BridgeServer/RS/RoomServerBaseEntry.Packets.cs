using Newtonsoft.Json;
using NSL.Logger;
using NSL.Node.BridgeServer.Managers;
using NSL.Node.BridgeServer.Shared.Message;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS
{
    public abstract partial class RoomServerBaseEntry
    {
        async Task SignServerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request = RoomSignInRequestModel.ReadFullFrom(data);

            bool result = await client.Entry.RoomManager.TryRoomServerConnect(client, request);

            new RoomSignInResponseModel
            {
                Result = result,
                ServerIdentity = client.Id,
                IdentityData = request.IdentityData
            }.WriteFullTo(response);

            client.Send(response);
        }

        void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = RoomSignSessionRequestModel.ReadFullFrom(data);

            RoomSignSessionResponseModel result = new RoomSignSessionResponseModel();

            var session = client.GetSession(request.SessionIdentity);

            result.Result = session != null && session.RoomIdentity.Equals(request.RoomIdentity);

            if (result.Result)
            {
                session.Active = true;

                result.Options = session.StartupInfo.GetDictionary();
            }
            else
            {
                Logger?.AppendError($"Session {request.SessionIdentity} has invalid, contains = {session != null}, {session?.RoomIdentity} != {request.RoomIdentity}");
            }

            result.WriteFullTo(packet);

            client.Send(packet);
        }

        void SignSessionPlayerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request
                = RoomSignSessionPlayerRequestModel.ReadFullFrom(data);

            var result = new RoomSignSessionPlayerResponseModel();

            var session = client.GetSession(request.SessionId);

            result.ExistsSession = session != null;

            if (session != null)
            {
                result.ExistsPlayer = session.ValidatePlayer(request.PlayerId);
            }

            result.WriteFullTo(response);

            client.ServerOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, JsonConvert.SerializeObject(result));

            client.Network?.Send(response);
        }

        void RoomMessageReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var message = RoomMessageModel.ReadFullFrom(data);

            client.GetSession(message.SessionId)?.SendLobbyRoomMessage(message.Data);
        }

        void FinishRoomReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var message = RoomFinishMessageModel.ReadFullFrom(data);

            client.GetSession(message.SessionId)?.SendLobbyFinishRoom(message.Data, true);
        }
    }
}
