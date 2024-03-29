﻿using NSL.ConfigurationEngine;
using NSL.SocketServer;
using NSL.SocketServer.Utils;

namespace NSL.ServerOptions.Extensions.ConfigurationEngine
{
    public static class NetworkConfigurationExtension
    {
        public static ServerOptions<T> LoadConfigurationServerOptions<T>(this BaseConfigurationManager configuration, string networkNodePath)
            where T : IServerNetworkClient
        {
            var r = configuration.LoadConfigurationCoreOptions<ServerOptions<T>, T>(networkNodePath);

            r.Backlog = configuration.GetValue<int>($"{networkNodePath}.io.backlog");

            r.IpAddress = configuration.GetValue($"{networkNodePath}.io.ip");
            r.Port = configuration.GetValue<int>($"{networkNodePath}.io.port");

            return r;
        }
    }
}
