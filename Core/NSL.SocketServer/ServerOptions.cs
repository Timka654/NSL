using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.SystemPackets;
using NSL.SocketServer.Utils;
using NSL.SocketServer.Utils.SystemPackets;

namespace NSL.SocketServer
{
    public class ServerOptions : ServerOptions<BaseNetworkConnection> { }

    public class ServerOptions<TClient> : TypedCoreOptions<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        public ServerOptions()
        {
            ConnectionFactory = () => new TClient();
            LoadOptions();
        }

        protected virtual void LoadOptions()
        {
            AddPacket(AliveConnectionPacket.PacketId, new ServerAliveConnectionPacket<TClient>());
            AddPacket(SystemTime<TClient>.PacketId, new SystemTime<TClient>());
        }
    }

    public static class NetworkConfigurationExtension
    {
        public static ServerOptions<T> LoadConfigurationServerOptions<T>(this INSLConfiguration configuration, string networkNodePath)
            where T : BaseNetworkConnection, new()
        {
            var r = configuration.LoadConfigurationCoreOptions<ServerOptions<T>>(networkNodePath);

            r.Backlog   = configuration.GetValue<int>($"{networkNodePath}.io.backlog");
            r.IpAddress = configuration.GetValue($"{networkNodePath}.io.ip");
            r.Port      = configuration.GetValue<int>($"{networkNodePath}.io.port");

            return r;
        }
    }
}
