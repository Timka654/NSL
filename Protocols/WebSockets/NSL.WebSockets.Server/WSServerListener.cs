using NSL.SocketServer.Utils;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Net;

namespace NSL.WebSockets.Server
{
    /// <summary>Типизированная обёртка первого уровня — принимает WSServerOptions&lt;T&gt; и передаёт в движок.</summary>
    public class WSServerListener<T> : WSServerListener
        where T : BaseNetworkConnection, new()
    {
        public WSServerListener(WSServerOptions<T> options) : base(options, options.EndPoints) { }
    }

    /// <summary>Не-дженериковый движок WS-сервера. Работает с CoreOptions, создаёт соединения через ConnectionFactory.</summary>
    public class WSServerListener : INetworkListener
    {
        private HttpListener listener;
        private bool state;
        public bool State => state;

        private readonly CoreOptions serverOptions;
        private readonly IEnumerable<string> endPoints;

        public WSServerListener(CoreOptions options, IEnumerable<string> endPoints)
        {
            serverOptions = options;
            this.endPoints = endPoints;
        }

        private void Initialize()
        {
            listener = new HttpListener();
            foreach (var endPoint in endPoints)
                listener.Prefixes.Add(endPoint);
            listener.Start();
        }

        public void Run()
        {
            if (state)
                throw new Exception();
            Initialize();
            listener.BeginGetContext(Accept, listener);
            state = true;
        }

        public void Stop()
        {
            state = false;
            try { listener.Close(); }
            catch (Exception ex) { serverOptions.CallExceptionEvent(ex, null); }
            listener = null;
        }

        private async void Accept(IAsyncResult result)
        {
            if (!state)
                return;

            HttpListenerContext client = null;

            try
            {
                client = listener.EndGetContext(result);
                await new WSServerClient(client, serverOptions).RunPacketReceiver();
            }
            catch (Exception ex)
            {
                serverOptions.CallExceptionEvent(ex, null);
            }
            listener.BeginGetContext(Accept, listener);
        }

        public int GetListenerPort() => 0;

        public void Start() => Run();

        public CoreOptions GetOptions() => serverOptions;
    }
}
