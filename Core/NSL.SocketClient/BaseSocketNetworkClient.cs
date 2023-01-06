using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore.Utils.SystemPackets;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketClient
{
    public abstract class BaseSocketNetworkClient : INetworkClient
    {
        /// <summary>
        /// Буффер для хранения отправленных пакетов во время разрыва соединения
        /// </summary>
        private Queue<byte[]> WaitPacketBuffer { get; set; }

        public ClientOptions<BaseSocketNetworkClient> ClientOptions => Network.Options as ClientOptions<BaseSocketNetworkClient>;

        public void Send(OutputPacketBuffer packet)
        {
            Network.Send(packet);
        }

        /// <summary>
        /// Получить все пакеты из списка ожидания и уберает их с очереди
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte[]> GetWaitPackets()
        {
            IEnumerable<byte[]> result = WaitPacketBuffer?.ToArray() ?? new byte[0][];

            ClearWaitPacketBuffer();

            return result;
        }

        /// <summary>
        /// Копировать пакеты в буффер с новым подключением
        /// </summary>
        /// <param name="other_client"></param>
        public void CopyWaitPacketBuffer(BaseSocketNetworkClient other_client)
        {
            other_client.WaitPacketBuffer = WaitPacketBuffer;
        }

        public void ClearWaitPacketBuffer()
        {
            if (WaitPacketBuffer != null)
                WaitPacketBuffer.Clear();
        }

        #region PingPong

        private bool pingPongEnabled;

        private CancellationTokenSource pingPongTokenSource;

        /// <summary>
        /// Milliseconds
        /// Важно! работает только при запуске цикла сообщений PingPongEnabled
        /// </summary>
        public int Ping { get; protected set; }

        public bool PingPongEnabled
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

                await Task.Delay(AliveCheckTimeOut, token);
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

        public async void RunRecovery()
        {
            await Task.CompletedTask;
            //if (ClientOptions.OldSessionClientData == null)
            //{
            //    ClientOptions.RunRecoverySession OnReconnectEvent?.Invoke(MaxRecoveryTryTime, false);
            //    return;
            //}

            //for (int currentTry = 0; currentTry < MaxRecoveryTryTime && NetworkClient != null; currentTry++)
            //{
            //    var result = await NetworkClient.ConnectAsync();

            //    OnReconnectEvent?.Invoke(currentTry + 1, result);

            //    if (result)
            //        break;

            //    await Task.Delay(RecoveryWaitTime);
            //}

            //byte[] buffer;

            //if (ClientData != null)
            //    while ((buffer = ClientData.GetWaitPacket()) != null)
            //    {
            //        NetworkClient.Send(buffer, 0, buffer.Length);
            //    }
        }

        public override void Dispose()
        {
            aliveLocker.Dispose();
            base.Dispose();
        }
    }
}
