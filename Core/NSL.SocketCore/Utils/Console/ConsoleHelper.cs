using NSL.Extensions.ConsoleEngine;
using NSL.SocketCore;
using NSL.SocketCore.Utils;

namespace NSL.ServerOptions.Extensions.Console
{
    public static class ConsoleHelper
    {
        public const ushort DefaultPacketId       = (ushort)NSLSystemPacketEnum.Console;
        public const ushort DefaultClientPacketId = (ushort)NSLSystemPacketEnum.Console;

        public static ConsoleManager<T> AddConsoleEngine<T>(this CoreOptions<T> _this, ushort packetId = DefaultPacketId)
            where T : INetworkClient
        {
            var m = new ConsoleManager<T>();
            _this.AddPacket(packetId, new ConsoleMessage<T>(m));
            return m;
        }
    }
}
