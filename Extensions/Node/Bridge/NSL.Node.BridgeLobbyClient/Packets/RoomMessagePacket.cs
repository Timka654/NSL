using NSL.Node.BridgeServer.Shared.Message;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeLobbyClient.Packets
{
    internal class RoomMessagePacket
    {
        public static async void Handle(BridgeLobbyNetworkClient client, InputPacketBuffer data)
        {
            var message = RoomMessageModel.ReadFullFrom(data);

            if (client.HandlesConfiguration.RoomFinishHandle != null)
                await client.HandlesConfiguration.RoomMessageHandle(message.SessionId, message.Data);
        }
    }
}
