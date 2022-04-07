using Microsoft.Extensions.Configuration;
using NSL.ConfigurationEngine;
using NSL.Logger;
using NSL.ServerOptions.Extensions;
using SocketPhantom.AspNetCore.Network.Packets;
using SocketPhantom.Cipher;
using SocketPhantom.Enums;
using SocketServer;

namespace SocketPhantom.AspNetCore.Network
{
    internal class PhantomNetworkServer : NetworkServer<PhantomHubClientProxy, PhantomNetworkServer>
    {
        protected override string ServerConfigurationName => "phantom";

        ILogger logger = FileLogger.Initialize();
        protected override ILogger Logger => logger;

        IConfigurationManager configurationManager;
        PhantomHubsManager manager;
        private PhantomCipherProvider cipher;

        protected override IConfigurationManager ConfigurationManager => configurationManager;

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
