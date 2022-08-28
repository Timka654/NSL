using NSL.BuilderExtensions.TCPClient;
using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;
using System.Reflection;
using System.Diagnostics;
using NSL.BuilderExtensions.TCPServer;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using ConsoleApp.Server;
using ConsoleApp.Client;

namespace ConsoleApp
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