using NSL.SocketCore.Utils;
using NSL.SocketCore.Network.Session;
using NSL.SocketServer.Utils;
using System;
using System.Threading.Tasks;

namespace NSL.SocketServer.Utils.Session
{
    public class NSLSessionServerOptions
    {
        public const string ObjectBagKey       = NSLObjectBagKeys.SessionServerOptions;
        public const string DefaultSessionBagKey = NSLObjectBagKeys.SessionInfo;

        public string   ClientSessionBagKey { get; set; } = DefaultSessionBagKey;
        public TimeSpan CloseSessionDelay   { get; set; } = TimeSpan.FromSeconds(20);
    }

    public class NSLSessionServerOptions<TClient> : NSLSessionServerOptions where TClient : IServerNetworkClient
    {
        public delegate Task<bool> ClientValidateDelegate(TClient client);
        public delegate Task ChangeSessionDelegate(TClient client, NSLSessionInfo sessionInfo);

        public ChangeSessionDelegate OnRecoverySession { get; set; } = (c, s) => Task.CompletedTask;
        public ChangeSessionDelegate OnExpiredSession  { get; set; } = (c, s) => Task.CompletedTask;
        public ClientValidateDelegate OnClientValidate { get; set; } = (c) => Task.FromResult(true);
    }
}
