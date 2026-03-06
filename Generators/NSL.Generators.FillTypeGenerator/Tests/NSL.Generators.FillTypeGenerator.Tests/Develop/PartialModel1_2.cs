#if !DEVELOP

using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Partial
{

    [FillTypeGenerate(typeof(PartialModel3))]
    [FillTypeFromGenerate(typeof(PartialModel2))]
    public partial class PartialModel1
    {
        public int Id3 { get; set; }
        public int Id4 { get; set; }
    }
}

#endif