using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCWithBuilder.TCP.Example
{
    public struct testStruct
    {
        public int tsValue;

        //public testStruct2 tsInnerStructTest { get; set; }

        public override string ToString()
        {
            return (new { tsValue }).ToString();
        }
    }
    public struct testStruct2
    {
        public int ts2Value { get; set; }

        public string ts2sval;
    }
}
