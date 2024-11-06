#if !DEVELOP

using NSL.Generators.FillTypeGenerator.Attributes;
using NSL.Generators.FillTypeGenerator.Tests.From;
using NSL.Generators.FillTypeGenerator.Tests.NoEqMemberTypeWithEqNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.NoEqMemberTypeWithEqNames
{
    [FillTypeFromGenerate(typeof(DevClass2))]
    [FillTypeGenerate(typeof(DevClass3))]
    internal partial class DevClass1
    {
        [FillTypeGenerateIgnore(typeof(DevClass2))]
        //[FillTypeGenerateIgnore(typeof(DevClass3))]
        public DevEnum Evalue { get; set; }

        public int dValue { get; set; }

        public ct1 pc1Test { get; set; }

        public ct2 pc2Test { get; set; }
    }
}

#endif