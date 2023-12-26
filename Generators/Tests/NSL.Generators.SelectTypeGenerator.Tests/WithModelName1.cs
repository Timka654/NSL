#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    [SelectGenerate("abc1", "abc2")]
    public partial class WithModelName1
    {
        [SelectGenerateInclude("abc1")] public int Abc1 { get; set; }

        [SelectGenerateInclude("abc1", "abc2")] public int Abc2 { get; set; }

        [SelectGenerateInclude("abc2")] public int Abc3 { get; set; }

        [SelectGenerateInclude("abc2")] public int Abc4 { get; set; }

        [SelectGenerateInclude("abc2")] public WithModelName2 AbcModel1 { get; set; }

        [SelectGenerateInclude("abc2")] public List<WithModelName2> AbcModel1L { get; set; }

        [SelectGenerateInclude("abc2")] public WithModelName2[] AbcModel1A { get; set; }

        [SelectGenerateInclude("abc2")] public WithModelName3 AbcModel2 { get; set; }
    }
}
#endif