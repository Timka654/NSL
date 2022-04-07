using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SocketCore.Extensions.Buffer;
using SocketCore.Utils.Buffer;
using SocketPhantom.AspNetCore.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SocketPhantom.AspNetCore
{
    public class BasePhantomHub<THub> : BasePhantomHub
        where THub : PhantomHub
    {
        internal IServiceProvider ServiceProvider { get; }

        private Dictionary<string, Func<object, InputPacketBuffer, Task>> methodDelegates = new();

        private Func<IServiceProvider, THub> instanceFunc;

        private Timer clearTimer;

        public BasePhantomHub(IServiceProvider provider, bool requiredAuth, PhantomHubOptions options) : base(requiredAuth, options)
        {
            ServiceProvider = provider;

            clearTimer = new Timer(ClearClients);

            clearTimer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

            LoadConstructor();

            LoadMethods();

            ((PhantomHubCallerClients)Clients).SetHub(this);
        }

        private void LoadConstructor()
        {
            var cnstr = typeof(THub).GetConstructors().First();

            var args = GetConstructorArgsAction(cnstr);

            instanceFunc = (sp) => cnstr.Invoke(args.Select(x => x(sp)).ToArray()) as THub;
        }

        private void LoadMethods()
        {
            var methods = typeof(THub).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public);

            foreach (var item in methods)
            {
                if (item.ReturnType == typeof(Task))
                    LoadTaskMethod(item);
                else
                    LoadMethod(item);
            }
        }

        private void LoadTaskMethod(MethodInfo methodInfo)
        {
            var getArgsAction = GetArgsAction(methodInfo);

            Func<object, InputPacketBuffer, Task> method = (_hub, _buf) => methodInfo.Invoke(_hub, getArgsAction.Select(x => x(_buf)).ToArray()) as Task;

            methodDelegates.Add($"{methodInfo.Name.ToLower()}_{methodInfo.GetParameters().Length}", method);
        }

        private void LoadMethod(MethodInfo methodInfo)
        {
            var getArgsAction = GetArgsAction(methodInfo);

            Func<object, InputPacketBuffer, Task> method = (_hub, _buf) => Task.Run(() => methodInfo.Invoke(_hub, getArgsAction.Select(x => x(_buf)).ToArray()));

            methodDelegates.Add($"{methodInfo.Name.ToLower()}_{methodInfo.GetParameters().Length}", method);
        }

        private List<Func<InputPacketBuffer, object>> GetArgsAction(MethodBase methodInfo)
        {
            var args = methodInfo.GetParameters();

            List<Func<InputPacketBuffer, object>> argsActions = new();

            foreach (var item in args)
            {
                var p = item;

                Func<InputPacketBuffer, object> action = (data) =>
                {
                    return ReadParameter(data, p.ParameterType);
                };

                argsActions.Add(action);
            }

            return argsActions;
            //return Action.Combine(argsActions.Cast<Delegate>().ToArray());
        }

        private List<Func<IServiceProvider, object>> GetConstructorArgsAction(MethodBase methodInfo)
        {
            var args = methodInfo.GetParameters();

            List<Func<IServiceProvider, object>> argsActions = new();

            foreach (var item in args)
            {
                var p = item;

                Func<IServiceProvider, object> action = (data) =>
                {
                    return data.GetService(p.ParameterType);
                };

                argsActions.Add(action);
            }

            return argsActions;
            //return Action.Combine(argsActions.Cast<Delegate>().ToArray());
        }

        public override async Task<bool> Invoke(PhantomHubClientProxy client, InputPacketBuffer packet)
        {
            using (var ip = new InputPacketBuffer())
            {
                packet.CopyTo(ip);

                ip.Position = 0;

                var methodName = ip.ReadString16().ToLower();

                if (methodDelegates.TryGetValue($"{methodName}_{ip.ReadInt32()}", out var func))
                {
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var hub = instanceFunc(scope.ServiceProvider);

                        hub.SetBase(this, client);

                        await func(hub, ip);
                    }

                    return true;
                }
            }

            return false;
        }

        private object ReadParameter(InputPacketBuffer packet, Type type) => packet.ReadJson16(type);

        private SemaphoreSlim loadLocker = new SemaphoreSlim(1);

        public override bool LoadClient(PhantomHubClientProxy client)
        {
            if (string.IsNullOrWhiteSpace(client.Session))
                return false;

            if (!loadLocker.Wait(5_000))
                return false;

            try
            {
                client.Hub = this;

                if (ClientList.TryGetValue(client.Session, out var oldClient))
                {
                    if (oldClient == client)
                    {
                        loadLocker.Release();
                        return true;
                    }

                    if (oldClient.GetState())
                    {
                        loadLocker.Release();
                        return false;
                    }

                    if (oldClient.Network != null)
                    {
                        oldClient.Network = client.Network;

                        client.Network.ChangeUserData(oldClient);

                        oldClient.ChangeOwner(client);

                        loadLocker.Release();

                        return true;
                    }
                    else
                    {
                        oldClient.Network = client.Network;

                        client.Network.ChangeUserData(oldClient);

                        oldClient.ChangeOwner(client);

                        client = oldClient;
                    }
                }

                ClientList.TryAdd(client.Session, client);

                if (!string.IsNullOrWhiteSpace(oldClient?.UserId))
                {
                    if (UserList.TryGetValue(oldClient.UserId, out var users) && oldClient != null)
                        users.Remove(oldClient);
                    else
                    {
                        UserList.TryAdd(client.UserId, users = new List<PhantomHubClientProxy>());
                    }

                    users.Add(client);
                }

                loadLocker.Release();

                using (var scope = ServiceProvider.CreateScope())
                {
                    var hub = instanceFunc(scope.ServiceProvider);

                    hub.SetBase(this, client);

                    hub.OnConnectedAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                loadLocker.Release();
                throw;
            }

            return true;
        }

        public override string GetSession(HttpContext context, string? session)
        {
            return GetSession(context.User, session);
        }

        public override string GetSession(ClaimsPrincipal claims, string? session)
        {
            PhantomHubClientProxy client;


            clearClientsLocker.Wait();

            if (session != default)
            {
                if (ClientList.TryGetValue(session, out var oldClient))
                {
                    //fix for remove on recovery
                    oldClient.LastReceiveMessage = DateTime.UtcNow.AddSeconds(20);

                    clearClientsLocker.Release();

                    return oldClient.Session;
                }
            }

            clearClientsLocker.Release();

            client = new PhantomHubClientProxy();

            do
            {
                client.Session = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            } while (!ClientList.TryAdd(client.Session, client));

            client.Claims = claims;

            return client.Session;
        }

        internal override void DisconnectClient(PhantomHubClientProxy client)
        {
            if (string.IsNullOrWhiteSpace(client.Session))
                return;

            clearClientsLocker.Wait();

            if (disconnectedBag.Contains(client))
                return;

            disconnectedBag.Add(client);

            clearClientsLocker.Release();
        }

        private ConcurrentBag<PhantomHubClientProxy> disconnectedBag = new();

        private SemaphoreSlim clearClientsLocker = new SemaphoreSlim(1);

        private void ClearClients(object e)
        {
            if (disconnectedBag.IsEmpty)
                return;

            clearClientsLocker.Wait();

            PhantomHubClientProxy[] cliens = disconnectedBag.ToArray();

            disconnectedBag = new ConcurrentBag<PhantomHubClientProxy>();

            clearClientsLocker.Release();

            var timeMark = DateTime.UtcNow.Add(-Options.DisconnectTimeOut);

            foreach (var client in cliens)
            {
                if (client.GetState())
                    continue;

                if (client.LastReceiveMessage > timeMark)
                {
                    DisconnectClient(client);
                    continue;
                }

                if (ClientList.TryGetValue(client.Session, out var oldClient))
                {
                    if (oldClient == client)
                    {
                        ClientList.TryRemove(client.Session, out _);
                    }

                    if (!string.IsNullOrWhiteSpace(client.UserId) && UserList.TryGetValue(client.UserId, out var users))
                    {
                        users.Remove(client);
                    }


                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var hub = instanceFunc(scope.ServiceProvider);

                        hub.SetBase(this, client);

                        hub.OnDisconnectedAsync(null).Wait();
                    }
                }
            }
        }
    }
}
