using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.SocketServer.Utils;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NSL.BuilderExtensions.TCPServer.TCPServerEndPointBuilder.Create()
                .WithClientProcessor<BaseServerNetworkClient>()
                .WithOptions()
                .WithCode(b => {
                    //TestRepository._ConfigurePacketHandles(b.GetCoreOptions());
                })
                .Build();

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


    public partial class Param3Struct
    {
        public int D3 { get; set; }

        public int D4 { get; set; }
    }


    public partial class Param4Struct
    {
        public int D5 { get; set; }

        public int D6 { get; set; }
    }

    [NSLBIOType]
    public partial class Result1Struct
    {
        public int D5 { get; set; }
    }

    public partial class Result2Struct
    {
        public int D5 { get; set; }
    }
}
