using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
