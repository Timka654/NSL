using NSL.Generators.BinaryTypeIOGenerator.Shared;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [NSLBIOType]
    public partial class RoomSignSessionPlayerResponseModel
    {
        public bool ExistsSession { get; set; }

        public bool ExistsPlayer { get; set; }
    }
}
