using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName
{
    [FillTypeGenerate(typeof(WithModelName3))]
    public partial class WithModelName1
    {
        public Guid v0 { get; set; }

        public WithModelName2[] v1 { get; set; }

        public string s1 { get; set; }

        public string[] v2 { get; set; }
    }
}
