using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Extensions.Version
{
    [NSLBIOType("Response")]
    public partial class NSLVersionResult
    {
        [NSLBIOInclude("Response")]
        public string Version { get; set; }

        [NSLBIOInclude("Response")]
        public string MinVersion { get; set; }

        [NSLBIOInclude("Response")]
        public string RequireVersion { get; set; }

        [NSLBIOInclude("Response")]
        public bool InvalidByMinVersion { get; set; }

        [NSLBIOInclude("Response")]
        public bool InvalidByReqVersion { get; set; }

        public bool IsInvalid => InvalidByMinVersion || InvalidByReqVersion;
    }
}
