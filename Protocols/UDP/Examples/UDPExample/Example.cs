using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Cipher;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using System;
using System.Threading;

namespace UDPExample
{
    public class TestPacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Client: Receive {nameof(TestPacket)}");
        }
    }
    public class TestPacketS : IPacket<NetworkClientForServer>
    {
        public override void Receive(NetworkClientForServer client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Receiver: Receive {nameof(TestPacketS)}");



            using (var packet = new NSL.UDP.DgramPacket() { PacketId = 1 })
            {
                packet.WriteInt32(1);
                packet.WriteInt32(2);
                packet.WriteInt32(3);

                client.Send(packet);
            }
        }
    }
    public class NetworkClient : BaseSocketNetworkClient
    {

    }
    public class NetworkClientForServer : IServerNetworkClient
    {

    }

    public class Example<TOptions>
        where TOptions : CoreOptions, IBindingUDPOptions, new ()
    {
        protected TOptions options;

        protected void Initialize()
        {
            options = new TOptions();

            options.BindingIP = "0.0.0.0";

            options.ReceiveBufferSize = 1024;

            options.InputCipher = new PacketNoneCipher();

            options.OutputCipher = new PacketNoneCipher();
        }
    }

    public class SenderExample : Example<UDPClientOptions<NetworkClient>>
    {
        protected UDPNetworkClient<NetworkClient> client;
        public SenderExample()
        {
            base.Initialize();

            options.IpAddress = "127.0.0.1";

            options.Port = 5553;

            options.BindingPort = 9994;

            options.AddPacket(1, new TestPacket());


            Run();
        }


        private void Run()
        {
            client = new UDPNetworkClient<NetworkClient>(options);
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

    public class ReceiverExample : Example<UDPServerOptions<NetworkClientForServer>>
    {
        protected UDPServer<NetworkClientForServer> listener;

        public ReceiverExample()
        {
            base.Initialize();

            options.BindingPort = 5553;

            options.AddPacket(1, new TestPacketS());

            Run();
        }

        private void Run()
        {
            listener = new UDPServer<NetworkClientForServer>(options);

            listener.Start();
        }
    }
}
