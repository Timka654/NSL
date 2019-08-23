﻿using DBEngine;
using phs.Data.NodeHostServer.Managers;
using phs.Data.NodeHostServer.Network;
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
        public static ConsoleManager<NetworkNodeServerData> ConsoleManager => Misc.console;

        public static ConfigurationManager ConfigurationManager => Misc.config;

        public static ProxyServerManager ProxyServerManager => ProxyServerManager.Instance;

        public static NodePlayerManager NodePlayerManager => NodePlayerManager.Instance;
    }
}