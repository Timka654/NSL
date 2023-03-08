using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.UDP.Client.Info;
using NSL.UDP.Client.Interface;
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
    public class UDPListener<TClient, TOptions>
        where TClient : INetworkClient, new()
        where TOptions : CoreOptions<TClient>, IBindingUDPOptions
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
                            options.RunException(new StunExceptionInfo(
                                item,
                                null,
                                StunExceptionInfo.ErrorTypeEnum.DNSIPAddressParseError,
                                null), default);

                        var stunIP = dnsIPs.FirstOrDefault();

                        var stunEndPoint = new IPEndPoint(stunIP, item.Port);

                        StunInformation = STUNClient.Query(listener, stunEndPoint, options.StunQueryType);

                        if (StunInformation.QueryError != STUNQueryError.Success)
                            options.RunException(new StunExceptionInfo(item, StunInformation, StunExceptionInfo.ErrorTypeEnum.QueryResultError, stunEndPoint), default);
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

            for (int i = 0; i < 3; i++)
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

        protected async void RunReceiveAsync() => await RunReceive();

        protected CancellationTokenSource ListenerCTS;

        protected async Task RunReceive()
        {
            if (ListenerCTS.Token.IsCancellationRequested)
                return;

            var poolMem = ArrayPool<byte>.Shared.Rent(options.ReceiveBufferSize);

            //SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            //args.SetBuffer(poolMem);
            //args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //args.Completed += Args_Completed;

            try
            {
                var recv = await listener.ReceiveFromAsync(poolMem, SocketFlags.None, _blankEndpoint);

                Args_Completed(poolMem[..recv.ReceivedBytes], recv);
            }
            catch (SocketException sex)
            {
                options.RunException(sex, null);
                StopReceive();
            }
            catch (Exception ex)
            {
                options.RunException(ex, null);
                StopReceive();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(poolMem);
            }
        }

        protected virtual void Args_Completed(Span<byte> buffer, SocketReceiveFromResult e)
        {
        }
    }


}
