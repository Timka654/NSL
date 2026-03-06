#if !DEVELOP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Test2Model : Test2Model_p1
    {
        public string TestValue1 { get; set; }
    }

    public abstract partial class Test2Model_p1
    {
        public string TestValue2 { get; set; }

        public string TestValue3 { get; set; }
    }
}

#endif