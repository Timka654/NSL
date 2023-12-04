using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName
{
    [FillTypeGenerate(typeof(WithModelName2))]
    [FillTypeGenerate(typeof(WithModelName3))]
    [FillTypeGenerate(typeof(WithModelName2), "abc1", "abc2")]
    public partial class WithModelName1
    {
        [FillTypeGenerateInclude("abc1")] public int Abc1 { get; set; }

        [FillTypeGenerateInclude("abc1", "abc2")] public int Abc2 { get; set; }

        [FillTypeGenerateInclude("abc2")] public int Abc3 { get; set; }

        [FillTypeGenerateInclude("abc2"), FillTypeGenerateIgnore(typeof(WithModelName2))] public int Abc4 { get; set; }
    }
}
