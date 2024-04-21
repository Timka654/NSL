using NSL.Extensions.Session;
using NSL.Extensions.Session.Server.Packets;
using NSL.SocketCore;
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

        public NSLSessionManager(NSLSessionServerOptions<TClient> options, CoreOptions networkOptions)
        {
            this.options = options;
            this.networkOptions = networkOptions;
            removeSessionCycleCTS = new CancellationTokenSource();

            RunRemoveSessionCycle();
        }

        private async void RunRemoveSessionCycle()
        {
            while (true)
            {
                try
                {
                    if (!waitCloseQueue.TryDequeue(out var waitClose))
                    {
                        var delay = options.CloseSessionDelay.TotalMilliseconds * 0.9;

                        await Task.Delay((int)delay, removeSessionCycleCTS.Token);

                        continue;
                    }

                    if (!waitClose.DisconnectTime.HasValue)
                        continue;

                    var waitTime = waitClose.DisconnectTime - DateTime.UtcNow;

                    if (!waitTime.HasValue)
                        continue;

                    if (waitTime.Value > TimeSpan.Zero)
                        await Task.Delay(waitTime.Value, removeSessionCycleCTS.Token);

                    if (!waitClose.DisconnectTime.HasValue)
                        continue;

                    RemoveSession(waitClose.Session, true);

                    await options.OnExpiredSession(waitClose.Client, waitClose);
                }
                catch (TaskCanceledException) { return; }
                catch (Exception ex) { networkOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, ex.ToString()); }
            }
        }

        public bool IsValidSession(TClient client)
        {
            if (client == null)
                return false;

            var session = client.GetSessionInfo();

            if (session == null)
                return false;

            return true;
        }

        private async Task Server_OnClientDisconnectAsyncEvent(TClient client)
        {
            if (client == null)
                return;

            var session = client.GetSessionInfo();

            if (session == null)
            {
                await options.OnExpiredSession(client, session);

                return;
            }

            if (!sessionStorage.TryGetValue(session.Session, out var ssession))
            {
                session.DisconnectTime = DateTime.UtcNow;
                await options.OnExpiredSession(client, session);

                return;
            }

            if (ssession != null && ssession != session)
            {
                session.DisconnectTime = DateTime.UtcNow;
                await options.OnExpiredSession(client, session);

                return;
            }

            if (!await options.OnClientValidate(client))
            {
                session.DisconnectTime = DateTime.UtcNow;
                await options.OnExpiredSession(client, session);

                return;
            }

            session.DisconnectTime = client.DisconnectTime + options.CloseSessionDelay;
            waitCloseQueue.Enqueue(session);
        }

        public async Task<NSLRecoverySessionResult> TryRecovery(TClient client, string session, string[] keys)
        {
            if (!sessionStorage.TryGetValue(session, out var oldSession)
                || oldSession.RestoreKeys.Length != keys.Length
                || !keys.SequenceEqual(oldSession.RestoreKeys)
                || !oldSession.DisconnectTime.HasValue
                || (oldSession.DisconnectTime.HasValue && oldSession.DisconnectTime < DateTime.UtcNow))
            {
                return new NSLRecoverySessionResult() { Result = NSLRecoverySessionResultEnum.NotFound };
            }

            sessionStorage.TryRemove(session, out _);

            oldSession.DisconnectTime = default;

            keys = GenerateKeys();

            client.Network.ChangeUserData(oldSession.Client);

            client.ChangeOwner(oldSession.Client);

            var sessionInfo = new NSLServerSessionInfo<TClient>(oldSession.Client, keys)
            {
                Session = session,
                ExpiredSessionDelay = options.CloseSessionDelay
            };

            sessionStorage.TryAdd(session, sessionInfo);

            oldSession.Client.ObjectBag[options.ClientSessionBagKey] = sessionInfo;

            var result = new NSLRecoverySessionResult() { Result = NSLRecoverySessionResultEnum.Ok, SessionInfo = sessionInfo };

            await options.OnRecoverySession(oldSession.Client, sessionInfo);

            return result;
        }

        internal void RegisterServer(ServerOptions<TClient> server)
        {
            server.AddAsyncPacket(NSLRecoverySessionPacket<TClient>.PacketId, new NSLRecoverySessionPacket<TClient>());

            server.OnClientDisconnectAsyncEvent += Server_OnClientDisconnectAsyncEvent;
        }

        public NSLServerSessionInfo<TClient> CreateSession(TClient client)
        {
            client.ThrowIfObjectBagNull();

            var sessionInfo = new NSLServerSessionInfo<TClient>(client, GenerateKeys())
            {
                ExpiredSessionDelay = options.CloseSessionDelay
            };

            string session;

            while (!sessionStorage.TryAdd(session = Token.GenerateToken(40).ToString(), sessionInfo)) ;

            sessionInfo.Session = session;

            client.ObjectBag[options.ClientSessionBagKey] = sessionInfo;

            return sessionInfo;
        }

        public NSLServerSessionInfo<TClient> CreateSession(TClient client, string session, bool replaceOnExists = true)
        {
            client.ThrowIfObjectBagNull();

            var sessionInfo = new NSLServerSessionInfo<TClient>(client, GenerateKeys())
            {
                ExpiredSessionDelay = options.CloseSessionDelay
            };

            if (!sessionStorage.TryAdd(session, sessionInfo))
            {
                if (replaceOnExists)
                    sessionStorage.TryRemove(session, out _);
                else
                    return null;
            }

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

        public bool RemoveSession(string session, bool expired = false)
        {
            if (sessionStorage.TryRemove(session, out var s))
            {
                if (expired && !s.DisconnectTime.HasValue)
                {
                    sessionStorage.TryAdd(session, s);
                    return false;
                }
            }

            return true;
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
        private readonly CoreOptions networkOptions;
    }
}
