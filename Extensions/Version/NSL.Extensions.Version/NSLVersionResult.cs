using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.Version
{
    [NSLBIOType]
    public partial class NSLVersionResult
    {
        public string Version { get; set; }

        public string MinVersion { get; set; }

        public string RequireVersion { get; set; }

        public bool InvalidByMinVersion { get; set; }

        public bool InvalidByReqVersion { get; set; }

        public bool IsInvalid => InvalidByMinVersion || InvalidByReqVersion;
    }
}
