using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.Version
{
    [NSLBIOType]
    public partial class NSLVersionInfo
    {
        public const string ObjectBagKey = "NSL__VERSION";

        public string Version { get; set; }
    }
}
