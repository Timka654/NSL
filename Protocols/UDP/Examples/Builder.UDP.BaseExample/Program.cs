using NSL.BuilderExtensions.UDPClient;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;
using System.Reflection;
using System.Diagnostics;
using NSL.BuilderExtensions.UDPServer;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
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