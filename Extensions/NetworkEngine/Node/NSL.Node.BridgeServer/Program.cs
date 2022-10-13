using NSL.ConfigurationEngine;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.TS;

namespace NSL.Node.BridgeServer
{
    internal class Program
    {
        public static ILogger Logger { get; } = ConsoleLogger.Create();

        public static BaseConfigurationManager Configuration { get; } = new ConfigurationManager();

        static void Main(string[] args)
        {
            ClientServer.Run();
            LobbyServer.Run();
            TransportServer.Run();

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}