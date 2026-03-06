using NSL.SocketCore.Utils.Logger;
using NSL.UDP;
using NSL.UDP.Client;

namespace UDPExample
{
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
}
