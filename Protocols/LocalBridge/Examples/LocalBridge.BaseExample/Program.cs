using LocalBridge.Example.Shared;
using NSL.LocalBridge;
using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.TCP.Client;
using NSL.TCP.Server;
using System.Net.Sockets;
using System.Reflection.Metadata;

namespace LocalBridge.BaseExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");


            ServerOptions<TCPExampleServer> loptions = new ServerOptions<TCPExampleServer>();

            TestOptions.ConfigureListener(loptions);

            //TCPServerListener<TCPExampleServer> l = new TCPServerListener<TCPExampleServer>(loptions);


            ClientOptions<TCPExampleClient> coptions = new ClientOptions<TCPExampleClient>();

            TestOptions.ConfigureClient(coptions);

            //var c = new TCPNetworkClient<TCPExampleClient, ClientOptions<TCPExampleClient>>(coptions);

            // this client need for send from client to server
            var clientToServer = new LocalBridgeClient<TCPExampleClient, TCPExampleServer>(coptions);


            // this client need for send from server to client
            var serverToClient = new LocalBridgeClient<TCPExampleServer, TCPExampleClient>(loptions);



            clientToServer.SetOtherClient(serverToClient);


            TestOptions.ProcessTest(clientToServer, serverToClient);
        }
    }
}