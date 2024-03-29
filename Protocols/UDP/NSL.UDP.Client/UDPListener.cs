﻿using NSL.SocketServer.Utils;
using NSL.UDP.Info;
using NSL.UDP.Interface;
using STUN;
using System;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.UDP.Client
{
    public abstract class UDPListener<TClient, TOptions>
        where TClient : IServerNetworkClient, new()
        where TOptions : UDPClientOptions<TClient>, IBindingUDPOptions
    {
        private static readonly IPEndPoint _blankEndpoint = new IPEndPoint(IPAddress.Any, 0);

        protected readonly TOptions options;

        protected bool state = false;

        protected Socket listener;

        public Socket GetSocket()
            => listener;

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


            //         listener.ExclusiveAddressUse = false;
            //listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            listener.Bind(options.GetBindingIPEndPoint());

            options.BindingPort = listener.LocalEndPoint is IPEndPoint ipep ? ipep.Port : options.BindingPort;

            STUNClient.ReceiveTimeout = 1700;

            if (options.StunServers.Any())
            {
                foreach (var item in options.StunServers)
                {
                    try
                    {
                        var dnsIPs = Dns.GetHostAddresses(item.Address).OrderByDescending(x => x.AddressFamily == options.AddressFamily);

                        if (!dnsIPs.Any())
                            options.CallExceptionEvent(new StunExceptionInfo(
                                item,
                                null,
                                StunExceptionInfo.ErrorTypeEnum.DNSIPAddressParseError,
                                null), default);

                        var stunIP = dnsIPs.FirstOrDefault();

                        var stunEndPoint = new IPEndPoint(stunIP, item.Port);

                        StunInformation = STUNClient.Query(listener, stunEndPoint, options.StunQueryType);

                        if (StunInformation.QueryError != STUNQueryError.Success)
                            options.CallExceptionEvent(new StunExceptionInfo(item, StunInformation, StunExceptionInfo.ErrorTypeEnum.QueryResultError, stunEndPoint), default);
                        else
                            break;
                    }
                    catch (SocketException) { }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            if (afterBind != null)
                afterBind();

            state = true;

            for (int i = 0; i < options.ReceiveChannelCount; i++)
            {
                RunReceiveAsync();
            }

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

        /// <summary>
        /// Run new receive cycle
        /// </summary>
        public async void RunReceiveAsync() => await RunReceive(ListenerCTS.Token);
        /// <summary>
        /// Run new receive cycle
        /// </summary>
        public async void RunReceiveAsync(CancellationToken cancellationToken)
        {
            var source = CancellationTokenSource.CreateLinkedTokenSource(ListenerCTS.Token, cancellationToken);

            await RunReceive(source.Token);
        }

        protected async void RunReceiveIntern(CancellationToken cancellationToken)
        {
            await RunReceive(cancellationToken);
        }

        protected CancellationTokenSource ListenerCTS;

        protected async Task RunReceive(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var poolMem = ArrayPool<byte>.Shared.Rent(options.ReceiveBufferSize);

            try
            {
                var recv = await listener.ReceiveFromAsync(poolMem, SocketFlags.None, _blankEndpoint);

                Args_Completed(poolMem[..recv.ReceivedBytes], recv, token);
            }
            catch (SocketException sex)
            {
                options.CallExceptionEvent(sex, null);
                StopReceive();
            }
            catch (Exception ex)
            {
                options.CallExceptionEvent(ex, null);
                StopReceive();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(poolMem);
            }
        }

        protected abstract void Args_Completed(Span<byte> buffer, SocketReceiveFromResult e, CancellationToken token);
    }
}
