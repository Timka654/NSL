using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.TCP.Server
{
    /// <summary>Типизированная обёртка первого уровня — принимает ServerOptions&lt;T&gt; и передаёт в движок.</summary>
    public class TCPServerListener<T> : TCPServerListener
        where T : BaseNetworkConnection, new()
    {
        public TCPServerListener(CoreOptions options, bool legacyThread = false) : base(options, legacyThread) { }
    }

    /// <summary>Не-дженериковый движок TCP-сервера. Работает с CoreOptions, создаёт соединения через ConnectionFactory.</summary>
    public class TCPServerListener : INetworkListener
    {
        public event ReceivePacketDebugInfo<TCPServerClient> OnReceivePacket;
        public event SendPacketDebugInfo<TCPServerClient> OnSendPacket;

        private Socket listener;
        private bool state;
        public bool State => state;

        private CoreOptions serverOptions;
        private readonly bool legacyThread;

        public TCPServerListener(CoreOptions options, bool legacyThread = false)
        {
            serverOptions = options;
            this.legacyThread = legacyThread;
        }

        private void Initialize()
        {
            var bindEp = serverOptions.GetBindingEndPoint();

            if (!IPAddress.TryParse(bindEp.IpAddress, out var ip))
                throw new ArgumentException($"invalid connection ip {bindEp.IpAddress}", nameof(bindEp.IpAddress));

            if (serverOptions.AddressFamily == AddressFamily.Unspecified)
                serverOptions.AddressFamily = ip.AddressFamily;

            if (serverOptions.ProtocolType == ProtocolType.Unspecified)
                serverOptions.ProtocolType = ProtocolType.Tcp;

            listener = new Socket(serverOptions.AddressFamily, SocketType.Stream, serverOptions.ProtocolType);
            listener.Bind(new IPEndPoint(ip, bindEp.Port));

            if (listener.LocalEndPoint is IPEndPoint ipep)
            {
                bindEp.Port = ipep.Port;
                serverOptions.WithBindingEndPoint(bindEp);
            }

            listener.Listen(bindEp.Backlog);
        }

        public void Run()
        {
            if (state) throw new Exception();
            Initialize();
            listener.BeginAccept(Accept, listener);
            state = true;
        }

        public void Stop()
        {
            state = false;
            try { listener.Close(); listener.Dispose(); }
            catch (Exception ex) { serverOptions.CallExceptionEvent(ex, null); }
            listener = null;
        }

        private void Accept(IAsyncResult result)
        {
            if (!state) return;

            Socket client = null;
            try
            {
                client = listener.EndAccept(result);
                var c = new TCPServerClient(client, serverOptions, legacyThread);
                c.OnReceivePacket += OnReceivePacket;
                c.OnSendPacket += OnSendPacket;
                c.RunPacketReceiver();
            }
            catch (Exception ex)
            {
                serverOptions.CallExceptionEvent(ex, null);
            }

            if (state)
            {
                try { listener.BeginAccept(Accept, listener); }
                catch (Exception ex) { serverOptions.CallExceptionEvent(ex, null); }
            }
        }

        public int GetListenerPort() => serverOptions.GetBindingEndPoint().Port;

        public void Start() => Run();

        public CoreOptions GetOptions() => serverOptions;
    }
}