using NSL.Node.BridgeServer.Shared.Requests;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS
{
    public abstract partial class LobbyServerBaseEntry
    {
        void AddPlayerRequestReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = LobbyRoomPlayerAddRequestModel.ReadFullFrom(data);

            client.AddPlayerId(request.RoomId, request.PlayerId);

            client.Network.Send(packet);
        }

        async Task CreateRoomSessionRequestReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request = LobbyCreateRoomSessionRequestModel.ReadFullFrom(data);

            var result = await client.Entry.RoomManager.CreateRoomSession(client, request);

            result.WriteFullTo(response);

            client.Network.Send(response);
        }

        void RemovePlayerRequestReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = LobbyRoomPlayerRemoveRequestModel.ReadFullFrom(data);

            client.RemovePlayerId(request.RoomId, request.PlayerId);

            client.Network.Send(packet);
        }

        void SignSessionRequestReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = LobbySignInRequestModel.ReadFullFrom(data);

            client.Identity = request.Identity;

            bool result = client.Entry.LobbyManager.TryLobbyServerConnect(client, request);

            packet.WriteBool(result);

            client.Network.Send(packet);
        }
    }
}
