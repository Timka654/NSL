using System;
using System.Collections.Generic;
using System.Text;
using Cipher;
using Cipher.RC.RC4;
using DBEngine;
using SocketServer;
using ps.Data.NodeServer.Network;
using Cipher.SHA;
using Utils.Helper.DbCmdQueue;
using System.Threading.Tasks;
using Utils.Logger;
using Utils.Helper.Console;
using Utils.Helper.Configuration;
using Utils.Helper.Configuration.Info;
using Utils.Helper.DB;

namespace ps.Data
{
    /// <summary>
    /// Статические данные проекта, входная точка сервера
    /// </summary>
    public class Misc
    {
        private static readonly List<ConfigurationInfo> defaultConfigValues = new List<ConfigurationInfo>()
        {
            new ConfigurationInfo ("proxy/public.ip", "127.0.0.1", ""),
            new ConfigurationInfo ("proxy/max.client.count", "30", ""),
            new ConfigurationInfo ("network/node_server/io.ip", "0.0.0.0", ""),
            new ConfigurationInfo ("network/node_server/io.port", "5693", ""),
            new ConfigurationInfo ("network/node_server/io.buffer.size", "8196", ""),
            new ConfigurationInfo ("network/node_server/io.backlog", "100", ""),
            new ConfigurationInfo ("network/node_server/io.protocol", "TCP", ""),
            new ConfigurationInfo ("network/node_server/io.ipv", "4", ""),


            new ConfigurationInfo ("network/node_host_client/access/token", "7AAEAC313C8778E6C3B489898A497F15", ""),
            new ConfigurationInfo ("network/node_host_client/io.ip", "127.0.0.1", ""),
            new ConfigurationInfo ("network/node_host_client/io.port", "5680", ""),
            new ConfigurationInfo ("network/node_host_client/io.buffer.size", "8196", ""),
            new ConfigurationInfo ("network/node_host_client/io.backlog", "100", ""),
            new ConfigurationInfo ("network/node_host_client/io.protocol", "TCP", ""),
            new ConfigurationInfo ("network/node_host_client/io.ipv", "4", "")
        };

        public static ConfigurationManager config;

        public static ConsoleManager<NetworkClientData> console;

        /// <summary>
        /// Входная точка сервера, инициализация
        /// </summary>
        public static void Loading()
        {
            Utils.Logger.PerformanceLogger.Initialize();
            Utils.Logger.FileLogger.Initialize();

            LoggerStorage.Instance.main.AppendInfo($"Initialization Server v{GetVersion()} - Proxy Server");
            LoggerStorage.Instance.main.AppendInfo("==================================================================================");

            console = new ConsoleManager<NetworkClientData>();
            config = new ConfigurationManager(defaultConfigValues);

            LoggerStorage.Instance.main.AppendInfo("==================================================================================");

            Data.NodeHostClient.Network.Client.Load();
            LoggerStorage.Instance.main.AppendInfo("==================================================================================");
            Data.NodeServer.Network.Server.Load();

            LoggerStorage.Instance.main.AppendInfo("");

        }

        private static string GetVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build).AddSeconds(version.Revision * 2);
            return $"{version} ({buildDate})";
        }
    }
}
