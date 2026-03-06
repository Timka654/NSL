using NSL.SocketCore.Utils.Logger;
using NSL.UDP;
using NSL.UDP.Client;

namespace UDPExample
{
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
