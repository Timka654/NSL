using NSL.Node.BridgeServer.Shared.Message;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeLobbyClient.Packets
{
    internal class FinishRoomMessagePacket
    {
        public static async void Handle(BridgeLobbyNetworkClient client, InputPacketBuffer data)
        {
            var message = RoomFinishMessageModel.ReadFullFrom(data);

            if (client.HandlesConfiguration.RoomFinishHandle != null)
                await client.HandlesConfiguration.RoomFinishHandle(message.SessionId, message.Data);
        }
    }
}
