using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Shared.Requests;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Managers
{
    public class LobbyManager
    {
        public LobbyManager(string identityKey)
        {
            this.identityKey = identityKey;
        }

        public void OnDisconnectedLobbyServer(LobbyServerNetworkClient client)
        {
            if (client.Signed)
                connectedServers.Remove(client.Identity, out _);
        }

        public bool TryLobbyServerConnect(LobbyServerNetworkClient client, LobbySignInRequestModel request)
        {
            if (!object.Equals(request.IdentityKey, identityKey))
                return false;

            if(connectedServers.TryGetValue(client.Identity, out var oldClient))
            {
                if (oldClient.Network?.GetState() == true)
                {
                    return false;
                }

                client.LoadFrom(oldClient);

                connectedServers.TryRemove(client.Identity, out _);
            }

            connectedServers.TryAdd(client.Identity, client);

            client.Signed = true;

            return true;
        }

        public LobbyServerNetworkClient? GetLobbyById(string lobbyId)
        {
            connectedServers.TryGetValue(lobbyId, out var lobby);

            return lobby;
        }


        private ConcurrentDictionary<string, LobbyServerNetworkClient> connectedServers = new ConcurrentDictionary<string, LobbyServerNetworkClient>();
        private readonly string identityKey;
    }
}
