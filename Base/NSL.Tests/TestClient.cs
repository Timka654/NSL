using Cipher;
using NSL.TCP.Client;
using SocketClient;
using SocketCore.Utils.Cipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Tests
{
    internal class TestClient
    {
        public ClientOptions<SocketNetworkClient> clientOptions = new ClientOptions<SocketNetworkClient>();

        public TCPNetworkClient<SocketNetworkClient, ClientOptions<SocketNetworkClient>> Client;

        public TestClient() : this(null)
        {
        }

        public TestClient(Action<ClientOptions<SocketNetworkClient>> configure)
        {
            clientOptions.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;

            clientOptions.inputCipher = new PacketNoneCipher();
            clientOptions.outputCipher = new PacketNoneCipher();
            clientOptions.ReceiveBufferSize = 1024;

            clientOptions.IpAddress = "127.0.0.1";
            clientOptions.Port = 14363;
            clientOptions.ReceiveBufferSize = 1024;

            if (configure != null)
                configure(clientOptions);

            Client = new TCPNetworkClient<SocketNetworkClient, ClientOptions<SocketNetworkClient>>(clientOptions);
        }
    }
}
