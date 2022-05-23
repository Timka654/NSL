using Microsoft.Extensions.Configuration;
using NSL.ConfigurationEngine;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.SocketCore.Extensions.TCPServer;
using NSL.SocketPhantom.AspNetCore.Network.Packets;
using NSL.SocketPhantom.Cipher;
using NSL.SocketPhantom.Enums;
using NSL.SocketServer;

namespace NSL.SocketPhantom.AspNetCore.Network
{
    internal class PhantomNetworkServer : TCPNetworkServerEntry<PhantomHubClientProxy, PhantomNetworkServer>
    {
        protected override string ServerConfigurationName => "phantom";

        ILogger logger = new FileLogger();
        protected override ILogger Logger => logger;

        BaseConfigurationManager configurationManager;
        PhantomHubsManager manager;
        private PhantomCipherProvider cipher;

        protected override BaseConfigurationManager ConfigurationManager => configurationManager;

        public void Load(PhantomHubsManager manager, IConfiguration configuration, PhantomCipherProvider cipher)
        {
            this.cipher = cipher;
            this.manager = manager;
            configurationManager = new PhantomConfigurationManager($"server.{ServerConfigurationName}", configuration);
            base.Load();
        }

        protected override ServerOptions<PhantomHubClientProxy> LoadConfigurationAction()
        {
            var options = base.LoadConfigurationAction();

            cipher.SetProvider(options);

            return options;
        }

        protected override void LoadReceivePacketsAction()
        {
            options.AddPacket((byte)PacketEnum.SignIn, new SessionPacket(manager));
            options.AddPacket((byte)PacketEnum.Invoke, new InvokePacket());
        }

        protected override void SocketOptions_OnClientConnectEvent(PhantomHubClientProxy client)
        {
            base.SocketOptions_OnClientConnectEvent(client);
        }

        protected override void SocketOptions_OnClientDisconnectEvent(PhantomHubClientProxy client)
        {
            base.SocketOptions_OnClientDisconnectEvent(client);

            if (client?.Hub != null)
            {
                client.Hub.DisconnectClient(client);
            }
        }
    }
}
