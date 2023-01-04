using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Cipher;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using NSL.UDP.Client.Interface;
using System;
using System.Threading;

namespace UDPExample
{
    public class TestPacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Client: Receive {nameof(TestPacket)} - {data.ReadInt32()} - {data.ReadInt32()} - {data.ReadInt32()}");
        }
    }
    public class TestPacketS : IPacket<NetworkClientForServer>
    {
        public override void Receive(NetworkClientForServer client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Receiver: Receive {nameof(TestPacketS)} - {data.ReadInt32()} - {data.ReadInt32()} - {data.ReadInt32()}");



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

            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.l.google.com:19302"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun1.l.google.com:19302"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun2.l.google.com:19302"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun3.l.google.com:19302"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun4.l.google.com:19302"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.node4.co.uk"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.nventure.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.patlive.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.petcube.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.phoneserve.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.prizee.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.qvod.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.refint.net"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.remote-learner.net"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.rounds.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.samsungsmartcam.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.sysadminman.net"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.tatneft.ru"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.telefacil.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.ucallweconn.net"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.virtual-call.com"));
            options.StunServers.Add(new NSL.UDP.Client.Info.StunServerInfo("stun.voxgratia.org"));

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
