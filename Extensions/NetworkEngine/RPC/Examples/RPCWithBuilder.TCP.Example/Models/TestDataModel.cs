using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCWithBuilder.TCP.Example.Models
{
    public class TestDataModel
    {
        public int tdValue { get; set; }

        public TestData2Model InnerClassTest { get; set; }
    }
}
