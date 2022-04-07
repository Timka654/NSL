using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using SocketPhantom.AspNetCore.Network;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace SocketPhantom.AspNetCore
{
    public class PhantomHubCallerContext : HubCallerContext
    {
        public override string ConnectionId => clientProxy.Session;

        public override string UserIdentifier => clientProxy.UserId;

        public override ClaimsPrincipal User => clientProxy.Claims;

        public IDictionary<object, object> items = new Dictionary<object, object>();

        public override IDictionary<object, object> Items => items;

        public PhantomHubFuatureCollection features = new();

        public override IFeatureCollection Features => features;

        public CancellationToken connectionAborted = new CancellationToken();

        public override CancellationToken ConnectionAborted => connectionAborted;

        public override void Abort()
        {
            clientProxy.Network.Disconnect();
        }

        public PhantomHubCallerContext()
        {

        }

        internal void Initialize(PhantomHubClientProxy clientProxy)
        {
            this.clientProxy = clientProxy;
        }

        private PhantomHubClientProxy clientProxy;

    }
}
