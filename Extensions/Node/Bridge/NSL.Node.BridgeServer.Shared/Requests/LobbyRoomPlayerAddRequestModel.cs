using NSL.Generators.BinaryTypeIOGenerator.Shared;
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
