using Cipher;
using NSL.UDP.Client;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.Cipher;
using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPExample
{
    public class TestPacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
        }
    }
    public class NetworkClient : IServerNetworkClient
    {

    }

    public class Example
    {
        protected UDPOptions<NetworkClient> options;

        protected void Initialize()
        {
            options = new UDPOptions<NetworkClient>();

            options.IpAddress = "127.0.0.1";

            options.Port = 5553;

            options.BindingIP = "0.0.0.0";

            options.BindingPort = default;

            options.ReceiveBufferSize = 1024;

            options.inputCipher = new PacketNoneCipher();

            options.outputCipher = new PacketNoneCipher();

            options.AddPacket(1, new TestPacket());
        }
    }

    public class SenderExample : Example
    {
        protected UDPConnection<NetworkClient> client;
        public SenderExample()
        {
            base.Initialize();

            Run();
        }


        private void Run()
        {
            client = new UDPConnection<NetworkClient>(options);
            Thread.Sleep(1_500);
            client.Connect();

            using (var packet = new NSL.UDP.DgramPacket() { PacketId = 1 })
            {
                packet.WriteInt32(1);
                packet.WriteInt32(2);
                packet.WriteInt32(3);

                client.GetClient().Send(packet);
            }

        }
    }

    public class ReceiverExample : Example
    {
        protected UDPServer<NetworkClient> listener;

        public ReceiverExample()
        {
            base.Initialize();

            base.options.BindingPort = base.options.Port;

            Run();
        }

        private void Run()
        {
            listener = new UDPServer<NetworkClient>(options);

            listener.Start();
        }
    }
}
