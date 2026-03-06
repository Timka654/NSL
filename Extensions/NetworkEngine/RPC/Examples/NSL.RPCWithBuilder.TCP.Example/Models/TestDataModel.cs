//using NSL.Extensions.RPC.Generator.Attributes;

namespace RPCWithBuilder.TCP.Example.Models
{
    public class TestDataModel
    {
        //[RPCMemberIgnore]
        public int tdValue { get; set; }

        public TestData2Model InnerClassTest { get; set; }
    }
}
