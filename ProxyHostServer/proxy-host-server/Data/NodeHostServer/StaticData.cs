using DBEngine;
using phs.Data.NodeHostServer.Network;
using phs.Data.GameServer.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Configuration;
using Utils.Helper.Console;
using Utils.Helper.DbCmdQueue;

namespace phs.Data.NodeHostServer
{
    public class StaticData
    {
        public static ConsoleManager<NetworkClientData> ConsoleManager => Misc.console;

        public static GameServer.Managers.NodePlayerManager NodePlayerManager => GameServer.Managers.NodePlayerManager.Instance;

        public static ConfigurationManager ConfigurationManager => Misc.config;
    }
}
