namespace RPCWithBuilder.TCP.Example.Models
{
    public struct TestStructModel
    {
        public int tsValue;

        //public testStruct2 tsInnerStructTest { get; set; }

        public override string ToString()
        {
            return (new { tsValue }).ToString();
        }
    }
}
