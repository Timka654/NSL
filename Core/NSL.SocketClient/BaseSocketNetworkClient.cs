using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore.Utils.SystemPackets;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSL.SocketCore;

namespace NSL.SocketClient
{
    public class BasicNetworkClient : BaseSocketNetworkClient { }

    public abstract class BaseSocketNetworkClient : INetworkClient
    {
        /// <summary>
        /// Буффер для хранения отправленных пакетов во время разрыва соединения
        /// </summary>
        /// 
        public CoreOptions ClientOptions => Network.Options;

        public void Send(OutputPacketBuffer packet)
        {
            Network.Send(packet);
        }

        #region PingPong

        private bool pingPongEnabled;

        private CancellationTokenSource pingPongTokenSource;

        /// <summary>
        /// Milliseconds
        /// Важно! работает только при запуске цикла сообщений PingPongEnabled
        /// </summary>
        public int Ping { get; protected set; }

        public virtual bool PingPongEnabled
        {
            get => pingPongEnabled; set
            {
                if (value == pingPongEnabled)
                    return;

                pingPongEnabled = value;

                if (pingPongEnabled)
                {
                    if (pingPongTokenSource != null)
                        pingPongTokenSource.Cancel();

                    if (pingPongTokenSource == null)
                        pingPongTokenSource = new CancellationTokenSource();


                    RunAliveChecker(pingPongTokenSource.Token);
                }
                else
                    Ping = 0;
            }
        }

        private async void RunAliveChecker(CancellationToken token)
        {
            do
            {
                RequestPing();

                await Task.Delay(AliveCheckTimeOut / 2, token);
            }
            while (!token.IsCancellationRequested && pingPongEnabled && Network?.GetState() == true);
        }

        public void RequestPing()
        {
            if (!aliveLocker.WaitOne(0))
                return;

            if (Network != null)
            {
                aliveLocker.Reset();

                AliveConnectionPacket.SendRequest(Network);

                aliveRequestTime = DateTime.UtcNow;
            }
        }

        internal void PongProcess()
        {
            Ping = (int)((DateTime.UtcNow - aliveRequestTime.AddMilliseconds(-2)).TotalMilliseconds / 2);

            aliveLocker.Set();
        }

        private DateTime aliveRequestTime;

        private ManualResetEvent aliveLocker { get; set; } = new ManualResetEvent(true);

        #endregion

        #region ServerTime

        public TimeSpan ServerDateTimeOffset { get; internal set; }

        public DateTime ServerDateTime { get; internal set; }

        public DateTime LocalDateTime { get; internal set; }

        public DateTime? GetClientDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;

            return GetClientDateTime(dateTime.Value);
        }

        public DateTime GetClientDateTime(DateTime dateTime)
        {
            return dateTime + ServerDateTimeOffset;
        }

        public void RequestServerTimeOffset()
        {
            ClientSystemTimePacket.SendRequest(this.Network);
        }

        #endregion

        public override void Dispose()
        {
            aliveLocker.Dispose();
            base.Dispose();
        }
    }
}
