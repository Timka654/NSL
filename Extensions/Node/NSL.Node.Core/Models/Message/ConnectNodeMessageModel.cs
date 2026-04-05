using NSL.Generators.BinaryTypeIOGenerator.Shared;

namespace NSL.Node.Core.Models.Message
{
    [NSLBIOType]
    public partial class ConnectNodeMessageModel
    {
        public string NodeId { get; set; }
        public string Token { get; set; }
        public string EndPoint { get; set; }
    }
}
