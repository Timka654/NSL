using Builder.UDP.BaseExample.Server;
using Builder.UDP.BaseExample.Client;

namespace Builder.UDP.BaseExample
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