using NSL.Builder.WebSockets.BaseExample.Client;
using NSL.Builder.WebSockets.BaseExample.Server;

namespace NSL.Builder.WebSockets.BaseExample
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