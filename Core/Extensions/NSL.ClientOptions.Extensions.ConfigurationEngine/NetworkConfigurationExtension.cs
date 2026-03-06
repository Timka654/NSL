using NSL.ConfigurationEngine;
using NSL.SocketClient;
using NSL.ServerOptions.Extensions.ConfigurationEngine;

namespace NSL.ClientOptions.Extensions.ConfigurationEngine
{
    public static class NetworkConfigurationExtension
    {
        public static ClientOptions<T> LoadConfigurationClientOptions<T>(this BaseConfigurationManager configuration, string networkNodePath)
            where T : BaseSocketNetworkClient
        {
            var r = configuration.LoadConfigurationCoreOptions<ClientOptions<T>, T>(networkNodePath);

            r.IpAddress = configuration.GetValue($"{networkNodePath}.io.ip");
            r.Port = configuration.GetValue<int>($"{networkNodePath}.io.port");

            return r;
        }
    }
}
