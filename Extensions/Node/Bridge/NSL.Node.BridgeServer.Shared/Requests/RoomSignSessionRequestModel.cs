using NSL.Generators.BinaryTypeIOGenerator.Shared;
using System;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [NSLBIOType]
    public partial class RoomSignSessionRequestModel
    {
        public Guid SessionIdentity { get; set; }

        public Guid RoomIdentity { get; set; }
    }
}
