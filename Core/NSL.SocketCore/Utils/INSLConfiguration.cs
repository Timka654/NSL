using System.Net.Sockets;

namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Minimal configuration abstraction that NSL components depend on.
    /// Implemented by <c>NSL.ConfigurationEngine.BaseConfigurationManager</c>.
    /// </summary>
    public interface INSLConfiguration
    {
        /// <summary>Returns the raw string value for <paramref name="path"/>, or <see langword="null"/> if absent.</summary>
        string GetValue(string path);

        /// <summary>Returns the converted value for <paramref name="path"/>, or <paramref name="defaultValue"/> if absent.</summary>
        T GetValue<T>(string path, T defaultValue = default);
    }

    public static class NetworkConfigurationExtension
    {
        public static AddressFamily GetIPv(this INSLConfiguration configuration, string nodePath)
        {
            switch (configuration.GetValue<byte>(nodePath))
            {
                case 6:
                    return AddressFamily.InterNetworkV6;
                case 4:
                default:
                    return AddressFamily.InterNetwork;
            }
        }

        public static ProtocolType GetProtocolType(this INSLConfiguration configuration, string nodePath)
        {
            switch (configuration.GetValue<string>(nodePath))
            {
                case "udp":
                    return ProtocolType.Udp;
                case "tcp":
                default:
                    return ProtocolType.Tcp;
            }
        }

        public static T LoadConfigurationCoreOptions<T, TType>(this INSLConfiguration configuration, string networkNodePath)
            where T : CoreOptions<TType>, new()
            where TType : INetworkClient
        {
            return new T
            {
                AddressFamily = configuration.GetIPv($"{networkNodePath}.io.ipv"),
                ProtocolType = configuration.GetProtocolType($"{networkNodePath}.io.protocol"),
                ReceiveBufferSize = configuration.GetValue<int>($"{networkNodePath}.io.buffer.size"),
            };
        }
    }
}
