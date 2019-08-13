using DBEngine;
using ps.Data.NodeServer.Network;
using ps.Data.NodeHostClient.Managers;
using ps.Data.NodeHostClient.Network;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Configuration;
using Utils.Helper.Console;
using Utils.Helper.DbCmdQueue;

namespace ps.Data.NodeHostClient
{
    public class StaticData
    {
        public static ConsoleManager<NetworkClientData> ConsoleManager => Misc.console;

        public static ConfigurationManager ConfigurationManager => Misc.config;

        public static NetworkClient<NetworkNodeHostClientData> NodeHostNetwork => Network.Client.NetworkClient;

        public static NodePlayerManager NodePlayerManager => NodePlayerManager.Instance;
    }
}
