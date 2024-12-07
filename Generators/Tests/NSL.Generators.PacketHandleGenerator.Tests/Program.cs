using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    [NSLBIOType]
    public partial class Param1Struct
    {
        public int D1 { get; set; }

        public int D2 { get; set; }
    }

    [NSLBIOType]
    public partial class Param2Struct
    {
        public int D3 { get; set; }

        public int D4 { get; set; }
    }

    [NSLBIOType]
    public partial class Result1Struct
    {
        public int D5 { get; set; }
    }
}
