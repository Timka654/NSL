using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [NSLBIOType]
    public partial class RoomSignInRequestModel
    {
        public Guid Identity { get; set; }

        public Dictionary<string, string> IdentityData { get; set; }

        public string ConnectionEndPoint { get; set; }
    }
}
