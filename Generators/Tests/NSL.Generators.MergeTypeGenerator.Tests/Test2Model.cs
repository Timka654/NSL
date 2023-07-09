using NSL.Generators.MergeTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.MergeTypeGenerator.Tests
{
    internal class Test2Model
    {
        public string TestValue1 { get; set; }

        [MergeToTypeIgnore(typeof(Test2Model))] public string TestValue2 { get; set; }
    }
}
