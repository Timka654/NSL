using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.UDP.Packet
{
    public class UDPPingPacket<TClient> : IDisposable
    where TClient : IServerNetworkClient, IUDPClientWithPing<TClient>
    {
        public const ushort ReceivePingPacketId = ushort.MaxValue - 100;
        public const ushort SendPingPacketId = ushort.MaxValue - 101;
        private readonly TClient client;
        private readonly ushort requestPID;

        public UDPPingPacket(TClient client, ushort requestPID = SendPingPacketId)
        {
            this.client = client;
            this.requestPID = requestPID;
        }

        private bool pingPongEnabled;

        private CancellationTokenSource pingPongTokenSource;

        private DateTime aliveRequestTime;

        public int Ping { get; protected set; }

        public bool PingPongEnabled
        {
            get
            {
                return pingPongEnabled;
            }
            set
            {
                if (value == pingPongEnabled)
                {
                    return;
                }

                pingPongEnabled = value;

                if (pingPongTokenSource != null)
                {
                    pingPongTokenSource.Cancel();
                    pingPongTokenSource = null;
                }

                if (pingPongEnabled)
                {
                    pingPongTokenSource = new CancellationTokenSource();

                    RunAliveChecker(pingPongTokenSource.Token);
                }
                else
                {
                    Ping = 0;
                }
            }
        }

        private ManualResetEvent aliveLocker { get; set; } = new ManualResetEvent(initialState: true);

        private async void RunAliveChecker(CancellationToken token)
        {
            do
            {
                RequestPing();
                await Task.Delay(client.AliveCheckTimeOut / 2, token);
            }
            while (!token.IsCancellationRequested && pingPongEnabled && (client.Network?.GetState() ?? false));
        }

        public void RequestPing()
        {
            if (aliveLocker.WaitOne(0) && client.Network != null)
            {
                aliveLocker.Reset();
                client.Network.SendEmpty(requestPID);
                aliveRequestTime = DateTime.UtcNow;
            }
        }

        internal void PongProcess()
        {
            Ping = (int)((DateTime.UtcNow - aliveRequestTime.AddMilliseconds(-2.0)).TotalMilliseconds / 2.0);
            aliveLocker.Set();
        }

        public void Dispose()
        {
            if (PingPongEnabled)
                PingPongEnabled = false;
        }
    }

    public static class UDPPingPacketExtensions
    {
        public static void RegisterUDPPingHandle<TClient>(this ServerOptions<TClient> options, ushort requestPID = UDPPingPacket<TClient>.SendPingPacketId, ushort responsePID = UDPPingPacket<TClient>.ReceivePingPacketId)
        where TClient : IServerNetworkClient, IUDPClientWithPing<TClient>
        {
            options.AddHandle(requestPID, (client, data) =>
            {
                client.Network.SendEmpty(responsePID);
            });

            options.AddHandle(responsePID, (client, data) =>
            {
                client.PingPacket?.PongProcess();
            });
        }
    }

    public interface IUDPClientWithPing<TClient>
        where TClient : IServerNetworkClient, IUDPClientWithPing<TClient>
    {
        UDPPingPacket<TClient> PingPacket { get; }
    }
}