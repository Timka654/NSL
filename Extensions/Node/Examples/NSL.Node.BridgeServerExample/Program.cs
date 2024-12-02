using NSL.Node.BridgeServer;
using NSL.Node.BridgeServer.LS;

namespace NSL.Node.BridgeServerExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NodeBridgeServerEntryBuilder.Create()
                .WithConsoleLogger()
                .WithDefaultManagers(string.Empty);
                

            Console.WriteLine(">>> Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}