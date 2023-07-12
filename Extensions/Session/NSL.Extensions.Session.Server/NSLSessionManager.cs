﻿using NSL.Extensions.Session;
using NSL.Extensions.Session.Server.Packets;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Server
{
    public class NSLSessionManager<TClient> : IDisposable
        where TClient : IServerNetworkClient
    {
        public const string ObjectBagKey = "NSL__SESSION__MANAGER";

        private ConcurrentDictionary<string, NSLServerSessionInfo<TClient>> sessionStorage { get; } = new ConcurrentDictionary<string, NSLServerSessionInfo<TClient>>();

        private ConcurrentQueue<NSLServerSessionInfo<TClient>> waitCloseQueue { get; } = new ConcurrentQueue<NSLServerSessionInfo<TClient>>();

        private CancellationTokenSource removeSessionCycleCTS;

        public NSLSessionManager(NSLSessionServerOptions<TClient> options)
        {
            this.options = options;

            removeSessionCycleCTS = new CancellationTokenSource();

            RunRemoveSessionCycle();
        }

        private async void RunRemoveSessionCycle()
        {
            do
            {
                try
                {
                    if (!waitCloseQueue.TryDequeue(out var waitClose))
                    {
                        var delay = options.CloseSessionDelay.TotalMilliseconds / 4;

                        await Task.Delay((int)delay, removeSessionCycleCTS.Token);

                        continue;
                    }

                    if (!waitClose.DisconnectTime.HasValue)
                        continue;

                    var waitTime = waitClose.DisconnectTime - DateTime.UtcNow;

                    if (!waitTime.HasValue)
                        continue;

                    await Task.Delay(waitTime.Value, removeSessionCycleCTS.Token);

                    if (!waitClose.DisconnectTime.HasValue)
                        continue;

                    RemoveSession(waitClose.Session);

                    options.OnExpiredSession(waitClose.Client, waitClose);
                }
                catch (TaskCanceledException) { return; }
                catch { }
            } while (true);
        }

        private void Server_OnClientDisconnectEvent(TClient client)
        {
            if (client == null)
                return;

            var session = client.GetSessionInfo();

            if (session == null)
                return;

            session.DisconnectTime = client.DisconnectTime + options.CloseSessionDelay;

            waitCloseQueue.Enqueue(session);
        }

        public NSLRecoverySessionResult TryRecovery(TClient client, string session, string[] keys)
        {
            if (!sessionStorage.TryGetValue(session, out var oldSession)
                || oldSession.RestoreKeys.Length != keys.Length
                || !keys.SequenceEqual(oldSession.RestoreKeys))
            {
                return new NSLRecoverySessionResult() { Result = NSLRecoverySessionResultEnum.NotFound };
            }

            sessionStorage.TryRemove(session, out _);

            oldSession.DisconnectTime = default;

            keys = GenerateKeys();

            client.ChangeOwner(oldSession.Client);

            var sessionInfo = new NSLServerSessionInfo<TClient>(client, keys)
            {
                Session = session
            };

            sessionStorage.TryAdd(session, sessionInfo);

            client.ObjectBag[options.ClientSessionBagKey] = sessionInfo;

            var result = new NSLRecoverySessionResult() { Result = NSLRecoverySessionResultEnum.Ok, SessionInfo = sessionInfo };

            options.OnRecoverySession(client, sessionInfo);

            return result;
        }

        internal void RegisterServer(ServerOptions<TClient> server)
        {
            server.AddPacket(NSLRecoverySessionPacket<TClient>.PacketId, new NSLRecoverySessionPacket<TClient>());

            server.OnClientDisconnectEvent += Server_OnClientDisconnectEvent;
        }

        public NSLServerSessionInfo<TClient> CreateSession(TClient client)
        {
            client.ThrowIfObjectBagNull();

            var sessionInfo = new NSLServerSessionInfo<TClient>(client, GenerateKeys());

            string session;

            while (!sessionStorage.TryAdd(session = Token.GenerateToken(40).ToString(), sessionInfo)) ;

            sessionInfo.Session = session;

            client.ObjectBag[options.ClientSessionBagKey] = sessionInfo;

            return sessionInfo;
        }

        public NSLServerSessionInfo<TClient> CreateSession(TClient client, string session)
        {
            client.ThrowIfObjectBagNull();

            var sessionInfo = new NSLServerSessionInfo<TClient>(client, GenerateKeys());

            if (!sessionStorage.TryAdd(session, sessionInfo))
                return null;

            sessionInfo.Session = session;

            client.ObjectBag[options.ClientSessionBagKey] = sessionInfo;

            return sessionInfo;
        }

        public bool RemoveSession(TClient client)
        {
            if (client == null)
                return false;

            client.ThrowIfObjectBagNull();

            var session = client.ObjectBag.Get<NSLSessionInfo>(options.ClientSessionBagKey);

            if (session == default)
                return false;

            return RemoveSession(session.Session);
        }

        public bool RemoveSession(string session)
        {
            return sessionStorage.TryRemove(session, out _);
        }

        public NSLServerSessionInfo<TClient> GetSession(string session)
            => sessionStorage.TryGetValue(session, out var s) ? s : default;

        private string[] GenerateKeys()
        {
            int count = rnd.Next(4, 8);

            string[] result = new string[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = Token.GenerateToken(24).ToString();
            }

            return result;
        }

        public void Dispose()
        {
            removeSessionCycleCTS.Cancel();
        }

        private static Random rnd = new Random();
        private readonly NSLSessionServerOptions<TClient> options;
    }
}
