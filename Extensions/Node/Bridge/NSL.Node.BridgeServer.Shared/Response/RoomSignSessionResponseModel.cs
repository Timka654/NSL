using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [NSLBIOType]
    public partial class RoomSignSessionResponseModel
    {
        public bool Result { get; set; }

        public Guid? RoomId { get; set; }

        public Dictionary<string, string> Options { get; set; }
    }
}
