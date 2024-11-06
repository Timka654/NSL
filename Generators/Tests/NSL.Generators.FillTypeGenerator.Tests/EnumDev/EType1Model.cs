#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Attributes;
using NSL.Generators.FillTypeGenerator.Tests.EnumDev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.EnumDev
{
    [FillTypeGenerate(typeof(EType2Model), "a1")]
    internal partial class EType1Model
    {
        [FillTypeGenerateInclude("a1")] public List<Dev1Enum> value { get; set; }
        [FillTypeGenerateInclude("a1")] public Dev1Enum value2 { get; set; }
    }

    public enum Dev1Enum
    {
        a1,
        a2
    }
}
#endif