#if !DEVELOP

using NSL.Generators.FillTypeGenerator.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeGenerate(typeof(PartialModel2))]
    [FillTypeFromGenerate(typeof(PartialModel3))]
    public partial class PartialModel1
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
    }

    public partial class PartialModel2
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }

        public int Id3 { get; set; }
        public int Id4 { get; set; }
    }

    public partial class PartialModel3
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }

        public int Id3 { get; set; }
        public int Id4 { get; set; }
    }
}

#endif