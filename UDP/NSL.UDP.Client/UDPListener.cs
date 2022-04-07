using SocketCore;
using SocketCore.Utils;
using SocketServer;
using SocketServer.Utils;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client
{
    public class UDPListener<TClient>
        where TClient : IServerNetworkClient, new()
    {
        protected readonly UDPOptions<TClient> options;

        protected bool state = false;

        protected Socket listener;

        public UDPListener(UDPOptions<TClient> options)
        {
            this.options = options;

            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
        }

        protected virtual void Options_OnClientDisconnectEvent(TClient client)
        {
        }

        protected void StartReceive(Action afterBind = null)
        {
            if (state)
                return;

            if (!IPAddress.TryParse(options.IpAddress, out var ip))
                throw new ArgumentException($"invalid connection ip {options.IpAddress}", nameof(options.IpAddress));

            if (options.AddressFamily == AddressFamily.Unspecified)
                options.AddressFamily = ip.AddressFamily;

            if (options.ProtocolType == ProtocolType.Unspecified)
                options.ProtocolType = ProtocolType.Udp;

            listener = new Socket(options.AddressFamily, SocketType.Dgram, options.ProtocolType);

            listener.Bind(options.GetBindingIPEndPoint());

            options.BindingPort = listener.LocalEndPoint is IPEndPoint ipep ? ipep.Port : options.BindingPort;

            if (afterBind != null)
                afterBind();

            for (int i = 0; i < 3; i++)
            {
                RunReceiveAsync();
            }

            state = true;
        }

        protected void StopReceive()
        {
            if (state == false)
                return;

            state = false;

            listener.Close();
            listener.Dispose();
            listener = null;
        }

        protected async void RunReceiveAsync() => await Task.Run(RunReceive);

        protected void RunReceive()
        {
            var poolMem = MemoryPool<byte>.Shared.Rent(options.ReceiveBufferSize);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            args.SetBuffer(poolMem.Memory);
            args.UserToken = poolMem;
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            args.Completed += Args_Completed;

            try
            {
                if (!listener.ReceiveFromAsync(args))
                    StopReceive();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected virtual void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
        }
    }


}
