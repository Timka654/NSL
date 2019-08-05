using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.SystemPackets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class IRecoverySessionManager<T> 
        where T : INetworkClient
    {
        protected ConcurrentDictionary<string, Tuple<string[], T>> keyStorage;

        protected TimeSpan WaitTime = TimeSpan.FromSeconds(20);

        public IRecoverySessionManager()
        {
            keyStorage = new ConcurrentDictionary<string, Tuple<string[], T>>();
        }

        public void RegisterServer(ServerOptions<T> server)
        {
            server.OnClientDisconnectEvent += Server_OnClientDisconnectEvent;
            server.OnRecoverySessionReceiveEvent += Server_OnRecoverySessionReceiveEvent;
        }

        private void Server_OnRecoverySessionReceiveEvent(T client, string session, string[] keys)
        {
            if (!keyStorage.TryRemove(session, out var result))
            {
                RecoverySession<T>.Send(client, RecoverySessionResultEnum.NotFound, null, null);
                return;
            }

            keys = GenerateKeys();

            result.Item2.ChangeOwner(client);
            result.Item2.SetRecoveryData(session, keys);

            keyStorage.TryAdd(session, new Tuple<string[], T>(keys, client));

            RecoverySession<T>.Send(client, RecoverySessionResultEnum.Ok, session, keys);
        }

        protected virtual void Server_OnClientDisconnectEvent(SocketServer.Utils.INetworkClient client)
        {
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
