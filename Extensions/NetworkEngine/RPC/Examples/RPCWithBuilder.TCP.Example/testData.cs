using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCWithBuilder.TCP.Example
{
    public class testData
    {
        public int tdValue { get; set; }

        public testData2 InnerClassTest { get; set; }
    }
    public class testData2
    {
        public int td2inval;
        public string td2sval;
    }
}
