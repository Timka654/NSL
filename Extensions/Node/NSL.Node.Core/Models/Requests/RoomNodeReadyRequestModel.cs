using NSL.Generators.BinaryTypeIOGenerator.Shared;
using System.Collections.Generic;

namespace NSL.Node.Core.Models.Requests
{
    [NSLBIOType]
    public partial class RoomNodeReadyRequestModel
    {
        public int ConnectedNodesCount { get; set; }

        public List<string> ConnectedNodes { get; set; }
    }
}
