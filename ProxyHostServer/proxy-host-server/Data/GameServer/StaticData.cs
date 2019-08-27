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
using phs.Data.NodeHostServer.Managers;

namespace phs.Data.GameServer
{
    public class StaticData
    {
        public static ConsoleManager<NetworkNodeServerData> ConsoleManager => Misc.console;

        public static ConfigurationManager ConfigurationManager => Misc.config;

        public static GameServerManager GameServerManager => GameServerManager.Instance;

        public static NodePlayerManager NodePlayerManager => NodePlayerManager.Instance;
    }
}
