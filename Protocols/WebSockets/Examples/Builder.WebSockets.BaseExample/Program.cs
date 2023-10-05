using Builder.WebSockets.BaseExample.Server;
using Builder.WebSockets.BaseExample.Client;

namespace Builder.WebSockets.BaseExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestWebSocketsServer.RunServer();

            //while (true)
            //{
            //    await Task.Delay(100);
            //}

            await TestWebSocketsClient.RunClient();

            await TestWebSocketsClient.RunTest();

            Console.WriteLine("Disconnecting");
            TestWebSocketsClient.Disconnect();

            Console.WriteLine("Press any key for exit");
            Console.ReadKey();
        }
    }
}