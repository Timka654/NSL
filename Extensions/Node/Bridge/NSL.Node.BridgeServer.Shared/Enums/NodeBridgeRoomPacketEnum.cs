namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeRoomPacketEnum : ushort
    {
        Response = 1,
        SignServerRequest,
        SignSessionRequest,
        FinishRoomMessage,
        RoomMessage,
        SignSessionPlayerRequest
    }
}
