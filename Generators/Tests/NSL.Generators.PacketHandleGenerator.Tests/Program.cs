using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.TCPServer;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.SocketClient;
using NSL.SocketServer.Utils;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var l = NSL.BuilderExtensions.TCPServer.TCPServerEndPointBuilder.Create()
                .WithClientProcessor<BaseServerNetworkClient>()
                .WithOptions()
                .WithBindingPoint(9996)
                .WithCode(b => {
                    ReceiveRepository.ConfigurePacketHandles(b.GetCoreOptions());
                })
                .Build();

            l.Start();

            var c = TCPClientEndPointBuilder.Create()
                .WithClientProcessor<BasicNetworkClient>()
                .WithOptions()
                .WithEndPoint("127.0.0.1", 9996)
                .WithCode(b =>
                {
                    b.GetOptions().ConfigureRequestProcessor();


                })
                .Build();


            SendRepositoryDelegateOutputStaticNetwork.client = c;
            SendRepositoryDelegateOutputStaticNetwork.requestProcessor = c.Data.GetRequestProcessor();


            ClientLog($"{nameof(SendRepositoryDelegateOutputStaticNetwork.SendPT2PacketRequest)} request");
            SendRepositoryDelegateOutputStaticNetwork.SendPT2PacketRequest(new Param3Struct() { D3 = 56, D4 = 66, }, new Param2Struct() { D4 = 55, D3 = 11 }, 9999, r =>
            {
                ClientLog($"{nameof(SendRepositoryDelegateOutputStaticNetwork.SendPT2PacketRequest)} response { r.D4} { r.D3}");
            });

            await Task.Delay(1000);

        }

        private static void ClientLog(string content)
        { 
        Console.WriteLine($"Client: ",content);
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
