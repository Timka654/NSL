using ConsoleEngine;
using SocketServer;
using SocketServer.Utils;

namespace NSL.ServerOptions.Extensions.Console
{
    public static class ConsoleHelper
    {
        public const ushort DefaultPacketId = ushort.MaxValue - 10;
        public const ushort DefaultClientPacketId = ushort.MaxValue - 10;

        public static ConsoleManager<T> AddConsoleEngine<T>(this ServerOptions<T> _this, ushort packetId = DefaultPacketId)
            where T : IServerNetworkClient
        {
            var m = new ConsoleManager<T>();
            _this.AddPacket(packetId, new ConsoleMessage<T>(m));
            return m;
        }
    }
}
