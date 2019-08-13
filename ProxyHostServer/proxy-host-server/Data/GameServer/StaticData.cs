using DBEngine;
using phs.Data.NodeHostServer.Network;
using phs.Data.GameServer.Managers;
using phs.Data.GameServer.Network;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Configuration;
using Utils.Helper.Console;
using Utils.Helper.DbCmdQueue;

namespace phs.Data.GameServer
{
    public class StaticData
    {
        public static ConsoleManager<NetworkClientData> ConsoleManager => Misc.console;

        public static ConfigurationManager ConfigurationManager => Misc.config;

        public static NetworkClient<NetworkNodeHostClientData> NodeHostNetwork => Network.Client.NetworkClient;

        public static NodePlayerManager NodePlayerManager => NodePlayerManager.Instance;
    }
}
