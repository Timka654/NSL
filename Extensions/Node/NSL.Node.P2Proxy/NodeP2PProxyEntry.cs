using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.P2Proxy.Client;
using NSL.UDP.Info;
using STUN;
using System;
using System.Linq;
using System.Net;

namespace NSL.Node.P2Proxy
{
    public abstract class NodeP2PProxyEntry
    {
        public abstract ILogger Logger { get; }

        public P2ProxyServerEntry ProxyServer { get; protected set; }

        //public string ClientPublicPoint => Configuration.GetValue<string>("client_public_endpoint", default(string));

        //public bool StunAutoDetect => Configuration.GetValue<bool>("client_stun_detect", default(bool));

        //protected string BuildClientPublicPoint(string address)
        //    => $"ws://{address}:{RoomServer.ClientBindingPort}/";

        public abstract void Run();

        //protected P2ProxyConfigurationManager CreateDefaultConfigurationManager()
        //    => new P2ProxyConfigurationManager(Logger);

        protected ILogger CreateConsoleLogger()
            => ConsoleLogger.Create();

        protected virtual P2ProxyServerEntry CreateClientServerNetwork()
            => ProxyServer = P2ProxyServerEntry
                .Create(this)
                .Run();

        //public static DefaultP2ProxyStartupEntry CreateDefault()
        //    => new DefaultP2ProxyStartupEntry();
    }
}
