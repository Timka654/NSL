using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Cipher;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.UDP;
using NSL.UDP.Client;
using NSL.UDP.Info;
using NSL.UDP.Interface;
using NSL.UDP.Packet;
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
    public class TestPacketS : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Receiver: Receive {nameof(TestPacketS)} - {data.ReadInt32()} - {data.ReadInt32()} - {data.ReadInt32()}");



            using (var packet = new NSL.UDP.DgramOutputPacketBuffer() { PacketId = 1 })
            {
                packet.WriteInt32(1);
                packet.WriteInt32(2);
                packet.WriteInt32(3);

                client.Send(packet);
            }
        }
    }
    public class NetworkClient : IServerNetworkClient, IUDPClientWithPing<NetworkClient>
    {
        public UDPPingPacket<NetworkClient> PingPacket { get; }

        public NetworkClient()
        {
            PingPacket = new UDPPingPacket<NetworkClient>(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            PingPacket.Dispose();
        }
    }

    public class Example<TOptions>
        where TOptions : UDPClientOptions<NetworkClient>, IBindingUDPOptions, new()
    {
        protected TOptions options;

        protected void Initialize(IBasicLogger logger)
        {
            options = new TOptions();

            options.BindingIP = "0.0.0.0";

            options.ReceiveBufferSize = 1024;

            options.HelperLogger = logger;

            options.StunServers.Add(new StunServerInfo("stun.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun1.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun2.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun3.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun4.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun.node4.co.uk"));
            options.StunServers.Add(new StunServerInfo("stun.nventure.com"));
            options.StunServers.Add(new StunServerInfo("stun.patlive.com"));
            options.StunServers.Add(new StunServerInfo("stun.petcube.com"));
            options.StunServers.Add(new StunServerInfo("stun.phoneserve.com"));
            options.StunServers.Add(new StunServerInfo("stun.prizee.com"));
            options.StunServers.Add(new StunServerInfo("stun.qvod.com"));
            options.StunServers.Add(new StunServerInfo("stun.refint.net"));
            options.StunServers.Add(new StunServerInfo("stun.remote-learner.net"));
            options.StunServers.Add(new StunServerInfo("stun.rounds.com"));
            options.StunServers.Add(new StunServerInfo("stun.samsungsmartcam.com"));
            options.StunServers.Add(new StunServerInfo("stun.sysadminman.net"));
            options.StunServers.Add(new StunServerInfo("stun.tatneft.ru"));
            options.StunServers.Add(new StunServerInfo("stun.telefacil.com"));
            options.StunServers.Add(new StunServerInfo("stun.ucallweconn.net"));
            options.StunServers.Add(new StunServerInfo("stun.virtual-call.com"));
            options.StunServers.Add(new StunServerInfo("stun.voxgratia.org"));

            options.RegisterUDPPingHandle();

            options.OnClientConnectEvent += Options_OnClientConnectEvent;
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;

            options.OnExceptionEvent += Options_OnExceptionEvent;

            options.InputCipher = new PacketNoneCipher();

            options.OutputCipher = new PacketNoneCipher();
        }

        private void Options_OnExceptionEvent(Exception ex, NetworkClient client)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Error, ex.ToString());
        }

        private void Options_OnClientConnectEvent(NetworkClient client)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Client Connected");
        }

        private void Options_OnClientDisconnectEvent(NetworkClient client)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Client disconnected");
        }
    }

    public class SenderExample : Example<UDPClientOptions<NetworkClient>>
    {
        protected UDPNetworkClient<NetworkClient> client;
        public SenderExample(IBasicLogger logger)
        {
            base.Initialize(logger);

            options.IpAddress = "127.0.0.1";

            options.Port = 5553;

            options.BindingPort = 9994;

            options.AddPacket(1, new TestPacket());

            options.OnClientConnectEvent += c => {
                c.PingPacket.PingPongEnabled = true;
            };


            Run();
        }


        private void Run()
        {
            client = new UDPNetworkClient<NetworkClient>(options);
            client.OnReceivePacket += Client_OnReceivePacket;
            client.OnSendPacket += Client_OnSendPacket;

            client.Connect();


            for (int i = 0; i < 10; i++)
            {
                using (var packet = new NSL.UDP.DgramOutputPacketBuffer() { PacketId = 1, Channel = NSL.UDP.Enums.UDPChannelEnum.ReliableUnordered })
                {
                    packet.WriteInt32((i * 3) + 1);
                    packet.WriteInt32((i * 3) + 2);
                    packet.WriteInt32((i * 3) + 3);

                    client.GetClient().Send(packet);
                }
            }

        }

        private void Client_OnSendPacket(UDPClient<NetworkClient> client, ushort pid, int len, string stacktrace)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"Send {pid}----{len}bytes to {client.GetRemotePoint()}");
        }

        private void Client_OnReceivePacket(UDPClient<NetworkClient> client, ushort pid, int len)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"Receive {pid}----{len}bytes from {client.GetRemotePoint()}");
        }
    }

    public class ReceiverExample : Example<UDPClientOptions<NetworkClient>>
    {
        protected UDPServer<NetworkClient> listener;

        public ReceiverExample(IBasicLogger logger)
        {
            base.Initialize(logger);

            options.BindingPort = 5553;

            options.AddPacket(1, new TestPacketS());

            Run();
        }

        private void Run()
        {
            listener = new UDPServer<NetworkClient>(options);

            listener.OnReceivePacket += Listener_OnReceivePacket;
            listener.OnSendPacket += Listener_OnSendPacket;

            listener.Start();
        }

        private void Listener_OnSendPacket(UDPClient<NetworkClient> client, ushort pid, int len, string stacktrace)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"Send {pid}----{len}bytes to {client.GetRemotePoint()}");
        }

        private void Listener_OnReceivePacket(UDPClient<NetworkClient> client, ushort pid, int len)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Debug, $"Receive {pid}----{len}bytes from {client.GetRemotePoint()}");
        }
    }
}
