using NSL.Builder.TCP.BaseExample.Client;
using NSL.Builder.TCP.BaseExample.Server;

namespace NSL.Builder.TCP.BaseExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestTcpServer.RunServer();

            await TestTcpClient.RunClient();

            await TestTcpClient.RunTest();

            TestTcpClient.Disconnect();

            Console.WriteLine("Press any key for exit");
            Console.ReadKey();
        }
    }
}