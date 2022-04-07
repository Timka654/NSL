using Microsoft.AspNetCore.SignalR;
using SocketPhantom.AspNetCore.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketPhantom.AspNetCore
{
    public class PhantomHubCallerClients : IHubCallerClients, IHubClients
    {
        public IClientProxy Caller => client;

        public IClientProxy others = default;

        public IClientProxy Others => others;

        public IClientProxy All => new PhantomTempClientProxy(mainHub.ClientList.Values);

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds)
        {
            return new PhantomTempClientProxy(mainHub.ClientList.Where(x => !excludedConnectionIds.Contains(x.Key)).Select(x => x.Value));
        }

        public IClientProxy Client(string connectionId)
        {
            mainHub.ClientList.TryGetValue(connectionId, out var client);

            return client;
        }

        public IClientProxy Clients(IReadOnlyList<string> connectionIds)
        {
            return new PhantomTempClientProxy(connectionIds.Select(x => Client(x)));
        }

        public IClientProxy Group(string groupName)
        {
            return ((PhantomGroupManager)mainHub.Groups).GetGroup(groupName);
        }

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds)
        {
            var group = ((PhantomGroupManager)mainHub.Groups).GetGroup(groupName);

            return new PhantomTempClientProxy(group.Clients.Where(x => !excludedConnectionIds.Contains(x.Key)).Select(x => x.Value));
        }

        public IClientProxy Groups(IReadOnlyList<string> groupNames)
        {
            return new PhantomTempClientProxy(groupNames.Select(x => Group(x)));
        }

        public IClientProxy OthersInGroup(string groupName)
        {
            var group = ((PhantomGroupManager)mainHub.Groups).GetGroup(groupName);

            return new PhantomTempClientProxy(group.Clients.Where(x => x.Key != client.Session).Select(x => x.Value));
        }

        public IClientProxy User(string userId)
        {
            mainHub.UserList.TryGetValue(userId, out var client);

            return new PhantomTempClientProxy(client);
        }

        public IClientProxy Users(IReadOnlyList<string> userIds)
        {
            return new PhantomTempClientProxy(mainHub.UserList.Where(x => userIds.Contains(x.Key)).SelectMany(x => x.Value));
        }

        public void SetHub(BasePhantomHub hub) => this.mainHub = hub;

        private BasePhantomHub mainHub;

        public PhantomHubCallerClients Clone(PhantomHubClientProxy client)
        {
            var caller = this.MemberwiseClone() as PhantomHubCallerClients;

            caller.client = client;
            caller.others = new PhantomOthersClientProxy(caller);

            return caller;
        }

        private PhantomHubClientProxy client;
    }
}
