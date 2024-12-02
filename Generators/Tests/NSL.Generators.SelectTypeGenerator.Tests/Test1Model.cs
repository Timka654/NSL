#if !DEVELOP

using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    [FillTypeGenerate(typeof(Test2Model))]
    public partial class Test1Model : Test1Model_p1
    {
        public string TestValue1 { get; set; }
    }

    public abstract partial class Test1Model_p1
    {
        public string TestValue2 { get; set; }

        [FillTypeGenerateIgnore(typeof(Test2Model))] public string TestValue3 { get; set; }
    }
}

#endif