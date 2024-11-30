using NSL.Generators.BinaryTypeIOGenerator.Attributes;
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
