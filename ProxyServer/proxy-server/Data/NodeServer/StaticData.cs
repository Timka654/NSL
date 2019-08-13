using DBEngine;
using ps.Data.NodeServer.Network;
using ps.Data.NodeHostClient.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Helper.Configuration;
using Utils.Helper.Console;
using Utils.Helper.DbCmdQueue;

namespace ps.Data.NodeServer
{
    public class StaticData
    {
        public static ConsoleManager<NetworkClientData> ConsoleManager => Misc.console;

        public static NodeHostClient.Managers.NodePlayerManager NodePlayerManager => NodeHostClient.Managers.NodePlayerManager.Instance;

        public static ConfigurationManager ConfigurationManager => Misc.config;
    }
}
