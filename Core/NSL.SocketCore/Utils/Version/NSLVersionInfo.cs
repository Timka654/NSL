using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.SocketCore.Utils.Version
{
    [NSLBIOType]
    public partial class NSLVersionInfo
    {
        public const string ObjectBagKey = NSLObjectBagKeys.Version;

        public string Version { get; set; }
    }
}
