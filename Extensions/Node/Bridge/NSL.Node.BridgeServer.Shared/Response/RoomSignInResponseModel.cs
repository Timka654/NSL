using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [NSLBIOType]
    public partial class RoomSignInResponseModel
    {
        public bool Result { get; set; }

        public Guid ServerIdentity { get; set; }

        public Dictionary<string, string> IdentityData { get; set; }
    }
}
