namespace RPCWithBuilder.TCP.Example.Models
{
    public struct TestStructModel
    {
        public int tsValue;


        public string? nValue;

        //public testStruct2 tsInnerStructTest { get; set; }

        public override string ToString()
        {
            return (new { tsValue, nValue }).ToString();
        }
    }
}
