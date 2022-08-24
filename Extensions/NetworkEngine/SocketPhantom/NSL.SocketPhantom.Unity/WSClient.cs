using NSL.Utils.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NSL.SocketPhantom.Unity
{
    public class WSClient : IDisposable
    {
        protected virtual string GetUrl() => "http://localhosts/hubs/hub";

        protected virtual WSRetryPolicy GetReconnectPolicy() => WSRetryPolicy.CreateNone();

        protected virtual string GetAccessToken() => string.Empty;

        PhantomHubConnection _hubConnection;

        public HubConnectionState CurrentState => _hubConnection.State;

        protected WSClient() : this((c, o) => { })
        { 
        
        }

        protected WSClient(Action<WSClient,PhantomConnectionOptions> optionsAction)
        {
            Build(new PhantomHubConnectionBuilder()
                .WithUrl(GetUrl, o => { o.AccessTokenProvider = () => Task.FromResult(GetAccessToken()); })
                .WithAutomaticReconnect(GetReconnectPolicy)
                .WithOptions(options => optionsAction(this, options)));
        }

        protected WSClient(PhantomHubConnectionBuilder builder)
        {
            Build(builder);
        }

        private void Build(PhantomHubConnectionBuilder builder)
        {
            _hubConnection = builder.Build();

            _hubConnection.OnException += (e) => { ThreadHelper.InvokeOnMain(() => OnException(e)); return Task.CompletedTask; };

            _hubConnection.StateChanged += (e) => ThreadHelper.InvokeOnMain(() => StateChanged(e));
        }

        public async Task Connect()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception e)
            {
                ThreadHelper.InvokeOnMain(() => OnException(e));
            }
        }

        public async void ConnectAsync()
            => await Connect();

        public async void ConnectAsync(Action<WSClient> afterConnect)
        {
            await Connect();
            ThreadHelper.InvokeOnMain(() => afterConnect(this));
        }

        #region On(Handle)

        public void Handle(string methodName, Action args)
        {
            _hubConnection.On(methodName, () =>
            {
                ThreadHelper.InvokeOnMain(() =>
                {
                    args();
                });
            });
        }

        public void Handle<T1>(string methodName, Action<T1> args)
        {
            _hubConnection.On<T1>(methodName, p1 =>
            {
                ThreadHelper.InvokeOnMain(() =>
                {
                    args(p1);
                });
            });
        }

        public void Handle<T1, T2>(string methodName, Action<T1, T2> args)
        {
            _hubConnection.On<T1, T2>(methodName, (p1, p2) =>
            {
                ThreadHelper.InvokeOnMain(() =>
                {
                    args(p1, p2);
                });
            });
        }

        public void Handle<T1, T2, T3>(string methodName, Action<T1, T2, T3> args)
        {
            _hubConnection.On<T1, T2, T3>(methodName, (p1, p2, p3) =>
            {
                ThreadHelper.InvokeOnMain(() =>
                {
                    args(p1, p2, p3);
                });
            });
        }

        public void Handle<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> args)
        {
            _hubConnection.On<T1, T2, T3, T4>(methodName, (p1, p2, p3, p4) =>
            {
                ThreadHelper.InvokeOnMain(() =>
                {
                    args(p1, p2, p3, p4);
                });
            });
        }

        #endregion

        #region Send

        public async void SendAsync(string methodName)
            => await _hubConnection.SendAsync(methodName);

        public async void SendAsync(string methodName, params object[] data)
            => await _hubConnection.SendAsync(methodName, data);

        public async void SendAsync(string methodName, Action<WSClient> afterSend)
        {
            await _hubConnection.SendAsync(methodName);
            ThreadHelper.InvokeOnMain(() => afterSend(this));
        }

        public async void SendAsync(string methodName, Action<WSClient, object[]> afterSend, params object[] data)
        {
            await _hubConnection.SendAsync(methodName, data);
            ThreadHelper.InvokeOnMain(() => afterSend(this, data));
        }

        #endregion

        public Task Disconnect() => _hubConnection.StopAsync();

        public async void DisconnectAsync()
            => await Disconnect();

        public async void DisconnectAsync(Action<WSClient> afterDisconnect)
        {
            await Disconnect();

            ThreadHelper.InvokeOnMain(() => afterDisconnect(this));
        }

        public async void Dispose()
            => await _hubConnection.DisposeAsync();

        protected virtual void OnException(Exception ex)
        {
            if (ex != null)
                Debug.LogException(ex);
        }

        protected virtual void StateChanged(HubConnectionState state)
            => Debug.LogWarning($"{nameof(StateChanged)} - {state}");
    }
}
