using NSL.Builder.UDP.BaseExample.Client;
using NSL.Builder.UDP.BaseExample.Server;

namespace NSL.Builder.UDP.BaseExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestUDPServer.RunServer();

            await TestUDPClient.RunClient();

            await TestUDPClient.RunTest();

            TestUDPClient.Disconnect();

            Console.WriteLine("Press any key for exit");
            Console.ReadKey();
        }
    }
}