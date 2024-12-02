using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [NSLBIOType]
    public partial class RoomSignSessionPlayerRequestModel
    {
        public Guid SessionId { get; set; }

        public string PlayerId { get; set; }
    }
}
