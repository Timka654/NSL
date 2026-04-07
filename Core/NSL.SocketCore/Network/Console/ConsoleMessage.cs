using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.Console
{
    public class ConsoleMessage<T> : IPacket<T> where T : INetworkClient
    {
        private readonly IConsoleManager<T> manager;
        private readonly ushort responsePacketId;

        public static ConsoleMessage<T> Instance { get; private set; }

        public ConsoleMessage(IConsoleManager<T> manager, ushort responsePacketId)
        {
            Instance = this;
            this.manager = manager;
            this.responsePacketId = responsePacketId;
        }

        public override void Receive(T client, InputPacketBuffer data)
        {
            Send(client, responsePacketId, manager.InvokeCommand(client, data.ReadString()));
        }

        public static void Send(INetworkClient client, ushort packetId, string result)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = packetId
            };

            packet.WriteString(result);

            client.Send(packet);
        }
    }
}
