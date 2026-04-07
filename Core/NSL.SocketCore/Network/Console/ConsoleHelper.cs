using Microsoft.Extensions.DependencyInjection;
using NSL.SocketCore;
using NSL.SocketCore.Utils;

namespace NSL.SocketCore.Utils.Console
{
    public static class ConsoleHelper
    {
        public static IConsoleManager<T> AddConsoleEngine<T>(this CoreOptions options, ushort packetId, IServiceCollection services, IConsoleManager<T> manager)
            where T : BaseNetworkConnection
        {
            options.AddPacket(packetId, new ConsoleMessage<T>(manager, packetId));
            services.AddSingleton(manager);
            return manager;
        }

        public static ConsoleManager<T> AddConsoleEngine<T>(this CoreOptions options, ushort packetId, IServiceCollection services)
            where T : BaseNetworkConnection
        {
            var m = new ConsoleManager<T>();
            options.AddPacket(packetId, new ConsoleMessage<T>(m, packetId));
            services.AddSingleton<IConsoleManager<T>>(m);
            services.AddSingleton(m);
            return m;
        }

        public static ConsoleManager<T> AddDefaultConsoleEngine<T>(this CoreOptions options, IServiceCollection services)
            where T : BaseNetworkConnection
            => options.AddConsoleEngine<T>((ushort)NSLSystemPacketEnum.Console, services);
    }
}
