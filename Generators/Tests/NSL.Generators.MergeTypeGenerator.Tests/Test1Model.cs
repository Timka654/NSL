using NSL.Generators.MergeTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.MergeTypeGenerator.Tests
{
    [MergeToType(typeof(Test2Model))]
    public partial class Test1Model : Test1Model_p1
    {
        public string TestValue1 { get; set; }
    }

    public abstract partial class Test1Model_p1
    {
        public string TestValue2 { get; set; }

        [MergeToTypeIgnore(typeof(Test2Model))] public string TestValue3 { get; set; }
    }
}
