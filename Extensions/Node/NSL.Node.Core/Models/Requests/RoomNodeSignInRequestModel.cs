using NSL.Generators.BinaryTypeIOGenerator.Shared;
using System;

namespace NSL.Node.Core.Models.Requests
{
    [NSLBIOType]
    public partial class RoomNodeSignInRequestModel
    {
        public Guid SessionId { get; set; }

        public Guid RoomId { get; set; }

        public string Token { get; set; }

        public string ConnectionEndPoint { get; set; }
    }
}
