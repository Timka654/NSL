using System;
using System.Collections.Generic;
using System.Text;
using Cipher;
using Cipher.RC.RC4;
using DBEngine;
using SocketServer;
using phs.Data.NodeHostServer.Network;
using Cipher.SHA;
using Utils.Helper.DbCmdQueue;
using System.Threading.Tasks;
using Utils.Logger;
using Utils.Helper.Console;
using Utils.Helper.Configuration;
using Utils.Helper.Configuration.Info;
using Utils.Helper.DB;

namespace phs.Data
{
    /// <summary>
    /// Статические данные проекта, входная точка сервера
    /// </summary>
    public class Misc
    {
        private static readonly List<ConfigurationInfo> defaultConfigValues = new List<ConfigurationInfo>()
        {
            new ConfigurationInfo ("network/game_server/access/token", "7AAEAC313C8778E6C3B489898A497F15", ""),
            new ConfigurationInfo ("network/game_server/io.ip", "0.0.0.0", ""),
            new ConfigurationInfo ("network/game_server/io.port", "5693", ""),
            new ConfigurationInfo ("network/game_server/io.buffer.size", "8196", ""),
            new ConfigurationInfo ("network/game_server/io.backlog", "100", ""),
            new ConfigurationInfo ("network/game_server/io.protocol", "TCP", ""),
            new ConfigurationInfo ("network/game_server/io.ipv", "4", ""),


            new ConfigurationInfo ("network/node_host_server/access/token", "7AAEAC313C8778E6C3B489898A497F15", ""),
            new ConfigurationInfo ("network/node_host_server/io.ip", "127.0.0.1", ""),
            new ConfigurationInfo ("network/node_host_server/io.port", "5680", ""),
            new ConfigurationInfo ("network/node_host_server/io.buffer.size", "8196", ""),
            new ConfigurationInfo ("network/node_host_server/io.backlog", "100", ""),
            new ConfigurationInfo ("network/node_host_server/io.protocol", "TCP", ""),
            new ConfigurationInfo ("network/node_host_server/io.ipv", "4", "")
        };

        public static ConfigurationManager config;

        public static ConsoleManager<NetworkNodeServerData> console;

        /// <summary>
        /// Входная точка сервера, инициализация
        /// </summary>
        public static void Loading()
        {
            Utils.Logger.PerformanceLogger.Initialize();
            Utils.Logger.FileLogger.Initialize();

            LoggerStorage.Instance.main.AppendInfo($"Initialization Server v{GetVersion()} - Proxy Server");
            LoggerStorage.Instance.main.AppendInfo("==================================================================================");

            console = new ConsoleManager<NetworkNodeServerData>();
            config = new ConfigurationManager(defaultConfigValues);

            LoggerStorage.Instance.main.AppendInfo("==================================================================================");

            Data.GameServer.Network.Server.Load();
            LoggerStorage.Instance.main.AppendInfo("==================================================================================");
            Data.NodeHostServer.Network.Server.Load();

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
