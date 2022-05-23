using NSL.ConfigurationEngine;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System.Net.Sockets;

namespace NSL.ServerOptions.Extensions.ConfigurationEngine
{
    public static class NetworkConfigurationExtension
    {
        /// <summary>
        /// Получить версию ип протокола
        /// </summary>
        /// <param name="ver">Номер версии ип протокола</param>
        /// <returns></returns>
        public static AddressFamily GetIPv(this BaseConfigurationManager configuration, string nodePath)
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

        /// <summary>
        /// Получить тип протокола Network
        /// </summary>
        /// <param name="name">Название протокола</param>
        /// <returns></returns>
        public static ProtocolType GetProtocolType(this BaseConfigurationManager configuration, string nodePath)
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

        public static T LoadConfigurationCoreOptions<T, TType>(this BaseConfigurationManager configuration, string networkNodePath)
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
