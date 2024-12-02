namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeLobbyPacketEnum : ushort
    {
        Response = 1,
        SignServerRequest,
        CreateRoomSessionRequest,
        AddPlayerRequest,
        RemovePlayerRequest,
        FinishRoomMessage,
        RoomChangeStateMessage,
        RoomMessage
    }
}
