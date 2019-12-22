using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerOptions.Extensions.Console
{
    public static class ConsoleHelper
    {
        public const ushort DefaultPacketId = ushort.MaxValue - 10;

        public static ConsoleEngine.ConsoleManager<T> AddConsoleEngine<T>(this SocketServer.ServerOptions<T> _this, ushort packetId = DefaultPacketId)
            where T : IServerNetworkClient
        {
            var m = new ConsoleEngine.ConsoleManager<T>();
            _this.AddPacket(packetId, new ConsoleMessage<T>(m));
            return m;
        }
    }
}
