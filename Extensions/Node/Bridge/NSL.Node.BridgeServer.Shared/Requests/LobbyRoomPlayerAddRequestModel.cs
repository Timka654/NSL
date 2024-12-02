using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [NSLBIOType]
    public partial class LobbyRoomPlayerAddRequestModel
    {
        public Guid RoomId { get; set; }

        public string PlayerId { get; set; }
    }
}
