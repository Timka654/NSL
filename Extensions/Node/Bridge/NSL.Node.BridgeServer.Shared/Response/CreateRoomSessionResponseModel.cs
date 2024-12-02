using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [NSLBIOType]
    public partial class CreateRoomSessionResponseModel
    {
        public bool Result { get; set; }

        public List<RoomServerPointInfo> ConnectionPoints { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
