#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    public partial class WithModelName2
    {
        public int Abc1 { get; set; }

        [SelectGenerateInclude("abc2")] public int Abc2 { get; set; }

        [SelectGenerateInclude("abc2")] public int Abc3 { get; set; }
    }
}
#endif