using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Message
{
    [NSLBIOType]
    public partial class RoomMessageModel
    {
        public Guid SessionId { get; set; }

        public byte[] Data { get; set; }

        public bool Manual { get; set; }
    }
}
