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
    public class IRecoverySessionManager<T> where T : INetworkClient
    {
        protected ConcurrentDictionary<long, Tuple<string[], T>> keyStorage;

        protected TimeSpan WaitTime = TimeSpan.FromSeconds(20);

        public IRecoverySessionManager()
        {
            keyStorage = new ConcurrentDictionary<long, Tuple<string[], T>>();
        }

        public void RegisterServer(ServerOptions<T> server)
        {
            server.OnClientDisconnectEvent += Server_OnClientDisconnectEvent;
            server.OnRecoverySessionReceiveEvent += Server_OnRecoverySessionReceiveEvent;
        }

        private void Server_OnRecoverySessionReceiveEvent(T client, long id, string[] keys)
        {
            if (!keyStorage.TryRemove(id, out var result))
            {
                RecoverySession<T>.Send(client, RecoverySessionResultEnum.NotFound, null, null);
                return;
            }

            keys = GenerateKeys(id);

            result.Item2.ChangeOwner(client);
            result.Item2.SetRecoveryData(id, keys);

            keyStorage.TryAdd(id, new Tuple<string[], T>(keys, client));

            RecoverySession<T>.Send(client, RecoverySessionResultEnum.Ok, id, keys);
        }

        protected virtual void Server_OnClientDisconnectEvent(SocketServer.Utils.INetworkClient client)
        {
            long id = client.GetId();

            if (id == 0)
                return;

            client.DisconnectTime = DateTime.Now + WaitTime;
        }

        protected virtual string[] GenerateKeys(long id)
        {
            Random rnd = new Random();

            int count = rnd.Next(4, 8);

            string[] result = new string[count];

            byte[] buffer = new byte[22];

            for (int i = 0; i < count; i++)
            {
                rnd.NextBytes(buffer);
                result[i] = string.Join("", buffer.Select(x => x.ToString("x2")));
            }

            return result;
        }

    }
}
