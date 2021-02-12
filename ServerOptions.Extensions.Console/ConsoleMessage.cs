using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsoleEngine;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using SocketServer.Utils;

namespace ServerOptions.Extensions.Console
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
            Send(client, manager.InvokeCommand(client, data.ReadString16()));
        }

        public static void Send(IServerNetworkClient client, string result)
        {
            var packet = new OutputPacketBuffer()
            {
                PacketId = ConsoleHelper.DefaultClientPacketId
            };

            packet.WriteString16(result);

            client.Send(packet);
        }
    }
}
