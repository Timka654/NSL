using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [NSLBIOType]
    public partial class RoomSignSessionPlayerResponseModel
    {
        public bool ExistsSession { get; set; }

        public bool ExistsPlayer { get; set; }
    }
}
