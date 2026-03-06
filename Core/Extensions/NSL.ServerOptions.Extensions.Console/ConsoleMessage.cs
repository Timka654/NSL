using NSL.Extensions.ConsoleEngine;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;

namespace NSL.ServerOptions.Extensions.Console
{
    public class ConsoleMessage<T> : IPacket<T> where T : IServerNetworkClient
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

        public static void Send(IServerNetworkClient client, string result)
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
