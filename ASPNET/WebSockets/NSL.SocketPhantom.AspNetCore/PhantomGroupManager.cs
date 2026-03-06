using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketPhantom.AspNetCore.Network
{
    public class PhantomGroupManager : IGroupManager
    {
        private ConcurrentDictionary<string, PhantomGroupClientProxy> groupsCollection = new ConcurrentDictionary<string, PhantomGroupClientProxy>();
        private readonly BasePhantomHub baseHub;

        public PhantomGroupManager(BasePhantomHub baseHub)
        {
            this.baseHub = baseHub;
        }

        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {

            if (!baseHub.ClientList.TryGetValue(connectionId, out var client))
                return Task.CompletedTask;

            var list = new PhantomGroupClientProxy();

            if (!groupsCollection.TryAdd(groupName, list))
            {
                if (groupsCollection.TryGetValue(groupName, out list))
                {
                }
            }

            list.Clients.TryAdd(connectionId, client);

            return Task.CompletedTask;
        }

        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (!groupsCollection.TryGetValue(groupName, out var list))
                return Task.CompletedTask;

            list.Clients.TryRemove(connectionId, out var _);

            return Task.CompletedTask;
        }

        public PhantomGroupClientProxy GetGroup(string groupName)
        {
            if (groupsCollection.TryGetValue(groupName, out var group))
            {
                return group;
            }

            return new PhantomGroupClientProxy();
        }
    }
}
