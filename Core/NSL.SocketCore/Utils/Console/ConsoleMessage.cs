using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.Console
{
    public class ConsoleMessage<T> : IPacket<T> where T : INetworkClient
    {
        private readonly ConsoleManager<T> manager;

        public static ConsoleMessage<T> Instance { get; private set; }

        public ConsoleMessage(ConsoleManager<T> manager)
        {
            Instance = this;
            this.manager = manager;
        }

        public override void Receive(T client, InputPacketBuffer data)
        {
            Send(client, manager.InvokeCommand(client, data.ReadString()));
        }

        public static void Send(INetworkClient client, string result)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = ConsoleHelper.DefaultClientPacketId
            };

            packet.WriteString(result);

            client.Send(packet);
        }
    }
}
