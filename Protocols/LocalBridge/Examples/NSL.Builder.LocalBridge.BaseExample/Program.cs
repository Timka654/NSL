using LocalBridge.Example.Shared;
using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.TCPServer;
using NSL.SocketClient;
using NSL.SocketServer;

namespace Builder.LocalBridge.BaseExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");


            var cbuilder = TCPClientEndPointBuilder.Create()
                .WithClientProcessor<TCPExampleClient>()
                .WithOptions<ClientOptions<TCPExampleClient>>()
                .WithCode(options =>
                {
                    TestOptions.ConfigureClient(options.GetOptions());
                });


            var lbuilder = TCPServerEndPointBuilder.Create()
                .WithClientProcessor<TCPExampleServer>()
                .WithOptions<ServerOptions<TCPExampleServer>>()
                .WithCode(options =>
                {
                    TestOptions.ConfigureListener(options.GetOptions());
                });

            var clientToServer = cbuilder
                .CreateLocalBridge<TCPExampleClient, TCPExampleServer>();

            var serverToClient = lbuilder
                .CreateLocalBridge(anotherClient: clientToServer);

            TestOptions.ProcessTest(clientToServer, serverToClient);

        }
    }
}