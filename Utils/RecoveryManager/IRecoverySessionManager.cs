using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.SystemPackets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.RecoveryManager
{
    public class IRecoverySessionManager<T> 
        where T : INetworkClient
    {
        protected ConcurrentDictionary<string, RecoverySessionInfo<T>> keyStorage;

        protected TimeSpan WaitTime = TimeSpan.FromSeconds(20);

        public IRecoverySessionManager()
        {
            keyStorage = new ConcurrentDictionary<string, RecoverySessionInfo<T>>();
        }

        public void RegisterServer(ServerOptions<T> server)
        {
            server.OnClientDisconnectEvent += Server_OnClientDisconnectEvent;
            server.OnRecoverySessionReceiveEvent += Server_OnRecoverySessionReceiveEvent;
        }

        public virtual void AddSession(T client)
        {
            var session = Token.GenerateToken(40).ToString();

            var keys = GenerateKeys();
            client.SetRecoveryData(session, keys);

            keyStorage.TryAdd(session, new RecoverySessionInfo<T>( client,keys));
        }

        public virtual bool RemoveSession(T client)
        {
            if (client != null && !string.IsNullOrEmpty(client.GetSession()))
                return RemoveSession(client.GetSession());
            return true;
        }

        public virtual bool RemoveSession(string session)
        {
            return keyStorage.TryRemove(session, out var dummy);
        }

        private void Server_OnRecoverySessionReceiveEvent(T client, string session, string[] keys)
        {
            if (!keyStorage.TryRemove(session, out var result) || result.RestoreKeys.Count() != keys.Length || !((IEnumerable<string>)keys).SequenceEqual(result.RestoreKeys))
            {
                RecoverySession<T>.Send(client, RecoverySessionResultEnum.NotFound, null, null);
                return;
            }

            keys = GenerateKeys();

            result.Client.ChangeOwner(client);
            result.Client.SetRecoveryData(session, keys);

            keyStorage.TryAdd(session, new RecoverySessionInfo<T>(client, keys));

            RecoverySession<T>.Send(client, RecoverySessionResultEnum.Ok, session, keys);
        }

        protected virtual void Server_OnClientDisconnectEvent(SocketServer.Utils.INetworkClient client)
        {
            if (client == null)
                return;

            string session = client.GetSession();

            if (string.IsNullOrEmpty(session))
                return;

            client.DisconnectTime = DateTime.Now + WaitTime;
        }

        protected virtual string[] GenerateKeys()
        {
            int count = rnd.Next(4, 8);

            string[] result = new string[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = Token.GenerateToken(24).ToString();
            }

            return result;
        }

        protected static Random rnd = new Random();
    }
}
