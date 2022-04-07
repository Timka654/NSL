using SocketClient.Utils.SystemPackets;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClient
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

        public bool PingPongEnabled
        {
            get => pingPongEnabled; set
            {
                if (value == pingPongEnabled)
                    return;

                pingPongEnabled = value;

                if (pingPongEnabled)
                {
                    if (pingPongTokenSource == null)
                        pingPongTokenSource = new CancellationTokenSource();

                    pingPongTokenSource.Cancel();

                    RunAliveChecker(pingPongTokenSource.Token);
                }
            }
        }

        private async void RunAliveChecker(CancellationToken token)
        {
            await Task.Delay(AliveCheckTimeOut, token);
            do
            {
                RequestPing();
                await Task.Delay(AliveCheckTimeOut, token);
            }
            while (Network.GetState() && pingPongEnabled && !token.IsCancellationRequested);
        }

        public void RequestPing()
        {
            if (LastReceiveMessage > DateTime.UtcNow.AddMilliseconds(-AliveCheckTimeOut))
            {
                AliveState = true;
                return;
            }

            if (!aliveLocker.WaitOne(0))
                return;

            aliveLocker.Reset();

            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ServerPacketEnum.AliveConnection
            };

            Send(packet);

            AliveState = aliveLocker.WaitOne(AliveCheckTimeOut);
        }

        internal void PongProcess()
        {
            aliveLocker.Set();
        }

        private AutoResetEvent aliveLocker { get; set; } = new AutoResetEvent(true);

        #endregion

        #region ServerTime

        public TimeSpan ServerDateTimeOffset { get; internal set; }

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
            var packet = new OutputPacketBuffer()
            {
                PacketId = (ushort)ServerPacketEnum.ServerTime
            };

            Send(packet);
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
    }
}
