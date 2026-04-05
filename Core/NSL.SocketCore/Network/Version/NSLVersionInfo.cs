using NSL.Generators.BinaryTypeIOGenerator.Shared;
using NSL.SocketCore.Utils;

namespace NSL.SocketCore.Network.Version
{
    [NSLBIOType]
    public partial class NSLVersionInfo
    {
        public const string ObjectBagKey = NSLObjectBagKeys.Version;

        public string Version { get; set; }
    }
}
