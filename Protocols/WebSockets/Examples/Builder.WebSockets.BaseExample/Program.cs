using NSL.BuilderExtensions.WebSocketsClient;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;
using System.Reflection;
using System.Diagnostics;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using Builder.WebSockets.BaseExample.Server;
using Builder.WebSockets.BaseExample.Client;

namespace Builder.WebSockets.BaseExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestWebSocketsServer.RunServer();

            await TestWebSocketsClient.RunClient();

            await TestWebSocketsClient.RunTest();

            TestWebSocketsClient.Disconnect();

            Console.WriteLine("Press any key for exit");
            Console.ReadKey();
        }
    }
}