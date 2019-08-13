using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Utils.Helper.Network
{
    public static class NetworkConfigurationExtension
    {
        /// <summary>
        /// Получить версию ип протокола
        /// </summary>
        /// <param name="ver">Номер версии ип протокола</param>
        /// <returns></returns>
        public static AddressFamily GetIPv(this Configuration.ConfigurationManager configuration, string nodePath)
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
        public static ProtocolType GetProtocolType(this Configuration.ConfigurationManager configuration, string nodePath)
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

        public static ServerOptions<T> LoadConfigurationServerOptions<T>(this Configuration.ConfigurationManager configuration, string networkNodePath)
            where T : INetworkClient
        {
            return new ServerOptions<T>
            {
                IpAddress = configuration.GetValue($"{networkNodePath}/io.ip"),
                Port = configuration.GetValue<int>($"{networkNodePath}/io.port"),
                Backlog = configuration.GetValue<int>($"{networkNodePath}/io.backlog"),
                AddressFamily = configuration.GetIPv($"{networkNodePath}/io.ipv"),
                ProtocolType = configuration.GetProtocolType($"{networkNodePath}/io.protocol"),
                ReceiveBufferSize = configuration.GetValue<int>($"{networkNodePath}/io.buffer.size"),
            };
        }
    }
}
