using Cipher;
using NSL.TCP.Server;
using SocketCore.Utils.Cipher;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Tests
{
    internal class TestServer
    {
        public ServerOptions<SocketServerNetworkClient> serverOptions = new ServerOptions<SocketServerNetworkClient>();

        public TCPServerListener<SocketServerNetworkClient> Server;

        public TestServer() : this(null)
        {
        }

        public TestServer(Action<ServerOptions<SocketServerNetworkClient>> configure)
        {
            serverOptions.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;

            serverOptions.inputCipher = new PacketNoneCipher();
            serverOptions.outputCipher = new PacketNoneCipher();

            serverOptions.IpAddress = "0.0.0.0";
            serverOptions.Port = 14363;
            serverOptions.ReceiveBufferSize = 1024;

            if (configure != null)
                configure(serverOptions);

            Server = new TCPServerListener<SocketServerNetworkClient>(serverOptions);// = new SocketClient<SocketNetworkClient, ClientOptions<SocketNetworkClient>>(serverOptions);
        }
    }
}
