using NSL.SocketCore.Utils.Session;
using NSL.Generators.BinaryTypeIOGenerator.Shared;
using System.Collections.Generic;

namespace NSL.Node.Core.Models.Response
{
    [NSLBIOType]
    public partial class RoomNodeSignInResponseModel
    {
        public bool Success { get; set; }

        public string? NodeId { get; set; }

        public Dictionary<string, string> Options { get; set; }

        public NSLSessionInfo SessionInfo { get; set; }
    }
}
