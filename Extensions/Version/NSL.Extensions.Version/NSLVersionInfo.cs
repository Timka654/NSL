using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Extensions.Version
{
    [NSLBIOType]
    public partial class NSLVersionInfo
    {
        public const string ObjectBagKey = "NSL__VERSION";

        public string Version { get; set; }
    }
}
