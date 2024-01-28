using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Server
{
    public class NSLSessionServerOptions
    {
        public const string ObjectBagKey = "NSL__SESSION__SERVEROPTIONS";
        public const string DefaultSessionBagKey = "NSL__SESSION__INFO";

        public string ClientSessionBagKey { get; set; } = DefaultSessionBagKey;

        public TimeSpan CloseSessionDelay { get; set; } = TimeSpan.FromSeconds(20);
    }

    public class NSLSessionServerOptions<TClient> : NSLSessionServerOptions
        where TClient : IServerNetworkClient
    {
        public delegate Task<bool> ClientValidateDelegate(TClient client);

        public delegate Task ChangeSessionDelegate(TClient client, NSLSessionInfo sessionInfo);

        public ChangeSessionDelegate OnRecoverySession { get; set; } = (client, sessionInfo) => Task.CompletedTask;

        public ChangeSessionDelegate OnExpiredSession { get; set; } = (client, sessionInfo) => Task.CompletedTask;

        /// <summary>
        /// Validate client on disconnect for wait reconnect
        /// Return <see cref="false"/> for expire session
        /// </summary>
        public ClientValidateDelegate OnClientValidate { get; set; } = (client) => Task.FromResult(true);
    }
}
