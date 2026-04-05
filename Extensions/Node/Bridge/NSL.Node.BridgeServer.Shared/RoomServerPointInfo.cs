using NSL.Generators.BinaryTypeIOGenerator.Shared;
using System;

namespace NSL.Node.BridgeServer.Shared
{
    [NSLBIOType]
    public partial class RoomServerPointInfo
    {
        public string Endpoint { get; set; }

        public Guid SessionId { get; set; }
    }
}
