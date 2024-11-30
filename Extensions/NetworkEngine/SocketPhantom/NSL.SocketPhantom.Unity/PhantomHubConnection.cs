using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSL.RestExtensions.Unity;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketPhantom.Unity.Network;

namespace NSL.SocketPhantom.Unity
{

    public class PhantomHubConnection : IDisposable
    {
        public string Session { get; private set; }
        public string Path { get; private set; }

        private PhantomConnectionOptions options;

        internal PhantomConnectionOptions Options => options;

        internal Task<string> GetAccessToken() => (options.AccessTokenProvider ?? new Func<Task<string>>(() => Task.FromResult(default(string))))();

        public static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(30.0);

        public static readonly TimeSpan DefaultHandshakeTimeout = TimeSpan.FromSeconds(15.0);

        public static readonly TimeSpan DefaultKeepAliveInterval = TimeSpan.FromSeconds(15.0);

        public TimeSpan ConnectionTimeout
        {
            get;
            set;
        } = DefaultConnectionTimeout;


        public TimeSpan KeepAliveInterval
        {
            get;
            set;
        } = DefaultKeepAliveInterval;


        public TimeSpan HandshakeTimeout
        {
            get;
            set;
        } = DefaultHandshakeTimeout;

        public event Func<Exception, Task> OnException = (_) => Task.CompletedTask;

        public event Action<HubConnectionState> StateChanged = (_) => { };

        private PhantomNetworkClient network;

        private HubConnectionState state = HubConnectionState.Disconnected;

        public HubConnectionState State
        {
            get => state;
            internal set
            {
                state = value;
                StateChanged(value);
            }
        }

        public PhantomHubConnection(PhantomConnectionOptions options)
        {
            this.options = options;

            this.network = new PhantomNetworkClient(this);

            this.network.OnException += async (ex, client) => await OnException(ex);
        }

        public async Task StartAsync()
        {
            await StartAsync(false);
        }
        internal async Task StartAsync(bool reconnect)
        {
            if (state == HubConnectionState.Connected)
                return;

            try
            {
                if (!await connectLocker.WaitAsync(0))
                    return;

                forceStopped = false;

                if (!reconnect)
                    state = HubConnectionState.Connecting;

                string endUrl = string.Empty;

                if (!string.IsNullOrWhiteSpace(Session))
                    endUrl += $"?session={Session}";

                if (options.AccessTokenProvider != null)
                {
                    var token = await options.AccessTokenProvider();

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        if (string.IsNullOrWhiteSpace(endUrl))
                            endUrl = "?";
                        else
                            endUrl += $"&";

                        endUrl += $"access_token={await options.AccessTokenProvider()}";
                    }
                }

                var url = await options.Url() + endUrl;

                DebugException($"Try request to {url}");

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);

                using (HttpClient hc = new UnityHttpClient())
                {
                    hc.Timeout = HandshakeTimeout;

                    var response = await hc.SendAsync(msg);

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();

                        DebugException($"{url} - Success Request {content}");

                        var data = JsonConvert.DeserializeObject<PhantomRequestResult>(content);

                        Session = data.Session;
                        Path = data.Path;

                        await network.InitializeClient(data);

                        await authLocker.WaitAsync();
                    }
                    else
                    {
                        throw new Exception("cannot connected");
                    }
                }

            }
            catch (Exception ex)
            {
                if (authLocker.CurrentCount == 0)
                    authLocker.Release();
                if (!reconnect)
                    SetState(HubConnectionState.Disconnected, ex);
            }

            connectLocker.Release();
        }

        public async Task StopAsync()
        {
            ForceStop(null);

            await Task.CompletedTask;
        }

        bool forceStopped = false;

        internal bool ForceStoppedState => forceStopped;

        internal void ForceStop(Exception err)
        {
            forceStopped = true;

            if (network.client?.GetState() == true)
            {
                network.client.Disconnect();

                if (err != null)
                    OnException(err);
            }
            else
                SetState(HubConnectionState.Disconnected, err);
        }

        public void Dispose() => ForceStop(null);

        public async Task DisposeAsync() => await Task.Run(() => Dispose());

        #region On

        private Dictionary<string, Func<InputPacketBuffer, Task>> methodDelegates = new Dictionary<string, Func<InputPacketBuffer, Task>>();

        internal async void Invoke(InputPacketBuffer packet)
        {
            using (var ip = new InputPacketBuffer(packet.PacketLength, packet.PacketId))
            {
                ip.SetData(packet.Data);

                string methodName = ip.ReadString().ToLower();

                if (methodDelegates.TryGetValue($"{methodName}_{ip.ReadInt32()}", out var func))
                {
                    await func(ip);
                }
                else
                {
                    ForceStop(new Exception($"Received method {methodName} not found with args"));
                }
            }
        }

        public void On(string methodName, Action handle)
        {
            methodDelegates.Add($"{methodName.ToLower()}_0", (_) =>
            {
                try
                {
                    handle();
                }
                catch (Exception ex)
                {
                    ForceStop(ex);
                }

                return Task.CompletedTask;
            });
        }

        public void On<T1>(string methodName, Action<T1> handle)
        {
            methodDelegates.Add($"{methodName.ToLower()}_1", packet =>
            {
                try
                {
                    handle(packet.ReadJson16<T1>());
                }
                catch (Exception ex)
                {
                    ForceStop(ex);
                }

                return Task.CompletedTask;
            });
        }

        public void On<T1, T2>(string methodName, Action<T1, T2> handle)
        {
            methodDelegates.Add($"{methodName.ToLower()}_2", packet =>
            {
                try
                {
                    handle(packet.ReadJson16<T1>(), packet.ReadJson16<T2>());
                }
                catch (Exception ex)
                {
                    ForceStop(ex);
                }

                return Task.CompletedTask;
            });
        }

        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handle)
        {
            methodDelegates.Add($"{methodName.ToLower()}_3", packet =>
            {
                try
                {
                    handle(packet.ReadJson16<T1>(), packet.ReadJson16<T2>(), packet.ReadJson16<T3>());
                }
                catch (Exception ex)
                {
                    ForceStop(ex);
                }

                return Task.CompletedTask;
            });
        }

        public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handle)
        {
            methodDelegates.Add($"{methodName.ToLower()}_4", packet =>
            {
                try
                {
                    handle(packet.ReadJson16<T1>(), packet.ReadJson16<T2>(), packet.ReadJson16<T3>(), packet.ReadJson16<T4>());
                }
                catch (Exception ex)
                {
                    ForceStop(ex);
                }

                return Task.CompletedTask;
            });
        }

        #endregion

        #region Send

        public async Task SendAsync(string methodName)
        {
            await SendAsync(methodName, new object[] { });
        }

        public async Task SendAsync(string methodName, params object[] args)
        {
            if (state != HubConnectionState.Connected)
                throw new Exception($"Current state is {state}, must be {nameof(HubConnectionState.Connected)} for send");
            await Task.Run(() =>
            {
                var packet = new OutputPacketBuffer() { PacketId = 2 };

                packet.WriteString16(methodName);

                packet.WriteCollection(args, (p, item) => { p.WriteJson16(item); });

                network.client.Send(packet);
            });
        }

        #endregion

        private SemaphoreSlim connectLocker = new SemaphoreSlim(1);

        private SemaphoreSlim authLocker = new SemaphoreSlim(0);

        internal void SetState(HubConnectionState state, Exception ex = null)
        {
            if (this.State == state)
                return;

            this.State = state;

            if (state == HubConnectionState.Connected)
                network.retryCount = 0;
            else if (state == HubConnectionState.Disconnected && ex != null && !(ex is SocketException))
                OnException(ex);


            if (authLocker.CurrentCount == 0)
                authLocker.Release();
        }

        internal async void DebugException(string msg)
        {
            if (options.DebugExceptions)
                await OnException(new PhantomDebugMessageException(msg));
        }

        public class PhantomRequestResult
        {
            public string Path { get; set; }

            public string Session { get; set; }
        }
    }
}
