using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.UDP.Client.Info;
using NSL.UDP.Client.Interface;
using STUN;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.UDP.Client
{
    public class UDPListener<TClient, TOptions>
        where TClient : INetworkClient, new()
        where TOptions : CoreOptions<TClient>, IBindingUDPOptions
    {
        protected readonly TOptions options;

        protected bool state = false;

        protected Socket listener;

        public STUNQueryResult StunInformation { get; private set; }

        public UDPListener(TOptions options)
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

            if (ListenerCTS != null)
                ListenerCTS.Cancel();

            ListenerCTS = new CancellationTokenSource();

            if (!IPAddress.TryParse(options.BindingIP, out var ip))
                throw new ArgumentException($"invalid connection ip {options.BindingIP}", nameof(options.BindingIP));


            if (options.AddressFamily == AddressFamily.Unspecified)
                options.AddressFamily = ip.AddressFamily;

            if (options.ProtocolType == ProtocolType.Unspecified)
                options.ProtocolType = ProtocolType.Udp;

            listener = new Socket(options.AddressFamily, SocketType.Dgram, options.ProtocolType);

            listener.Bind(options.GetBindingIPEndPoint());

            options.BindingPort = listener.LocalEndPoint is IPEndPoint ipep ? ipep.Port : options.BindingPort;

            STUNClient.ReceiveTimeout = 700;

            if (options.StunServers.Any())
            {
                foreach (var item in options.StunServers)
                {
                    StunInformation = STUNClient.Query(listener, new IPEndPoint(Dns.GetHostAddresses(item.Address).FirstOrDefault(), item.Port), STUNQueryType.ExactNAT);

                    if (StunInformation.QueryError != STUNQueryError.Success)
                        options.RunException(new StunExceptionInfo(StunInformation), default);
                }
            }

            if (afterBind != null)
                afterBind();

            for (int i = 0; i < 3; i++)
            {
                RunReceiveAsync(ListenerCTS.Token);
            }

            state = true;
        }

        protected void StopReceive()
        {
            if (state == false)
                return;

            state = false;

            ListenerCTS.Cancel();

            listener.Close();
            listener.Dispose();
            listener = null;
        }

        protected async void RunReceiveAsync(CancellationToken token) => await Task.Run(() => RunReceive(token), token);

        protected CancellationTokenSource ListenerCTS;

        protected void RunReceive(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

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
            catch (SocketException sex)
            {
                throw;
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
