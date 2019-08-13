using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketServer.Utils.Buffer;
using SocketServer.Utils.SystemPackets;

namespace SocketServer.Utils
{
    /// <summary>
    /// Класс для хранения пользовательских данных
    /// </summary>
    public class INetworkClient
    {
        public DateTime LastReceiveMessage { get; set; }

        public bool AliveState { get; set; }

        public ulong PingCount { get; set; }

        public int AliveWaitTime { get; set; } = 3000;

        public int AliveCheckTimeOut { get; set; } = 3000;

        public int TimeSyncTimeOut { get; set; } = 300000;

        public long Version { get; set; }

        public ManualResetEvent Alive_locker { get; set; } = new ManualResetEvent(true);

        internal string Session { get; set; }

        public string[] RecoverySessionKeyArray { get; set; }

        public DateTime? DisconnectTime { get; set; }

        /// <summary>
        /// Клиент для отправки данных, эта переменная обязательна
        /// </summary>
        public IClient Network { get; set; }

        /// <summary>
        /// Буффер для хранения отправленных пакетов во время разрыва соединения
        /// </summary>
        private Queue<byte[]> WaitPacketBuffer { get; set; }
        public TimeSpan ServerDateTimeOffset { get; internal set; }

        /// <summary>
        /// Инициализация хранилища пакетов во время разрыва соединения
        /// </summary>
        public void InitializeWaitPacketBuffer()
        {
            if (WaitPacketBuffer == null)
                WaitPacketBuffer = new Queue<byte[]>();
        }

        /// <summary>
        /// Добавить пакет в список ожидания восстановления подключения
        /// </summary>
        /// <param name="packet_data"></param>
        /// <param name="lenght"></param>
        internal void AddWaitPacket(byte[] packet_data, int offset, int lenght)
        {
            if (WaitPacketBuffer == null)
                return;

            if (offset == 0 && lenght == packet_data.Length)
                WaitPacketBuffer.Enqueue(packet_data);
            else
            {
                var packet = new byte[lenght - offset];
                Array.Copy(packet_data, offset, packet, 0, lenght);
                WaitPacketBuffer.Enqueue(packet);
            }

        }


        internal void Send(OutputPacketBuffer packet)
        {
            Network.Send(packet);
        }

        /// <summary>
        /// Получить пакет из списка ожидания
        /// </summary>
        /// <returns></returns>
        public byte[] GetWaitPacket()
        {
            if (WaitPacketBuffer == null || WaitPacketBuffer.Count == 0)
                return null;
            return WaitPacketBuffer.Dequeue();
        }

        /// <summary>
        /// Копировать пакеты в буффер с новым подключением
        /// </summary>
        /// <param name="other_client"></param>
        public void CopyWaitPacketBuffer(INetworkClient otherClient)
        {
            otherClient.WaitPacketBuffer = WaitPacketBuffer;
        }

        public virtual void ChangeOwner(INetworkClient client)
        {
            WaitPacketBuffer = client.WaitPacketBuffer;
        }

        public async void RunAliveChecker()
        {
            await Task.Delay(AliveCheckTimeOut);
            while (Network.GetState())
            {
                AliveConnection<INetworkClient>.Send(this);
                await Task.Delay(AliveCheckTimeOut);
            }
        }

        public async void RunSyncTime()
        {
            while (Network.GetState())
            {
                SocketServer.Utils.SystemPackets.SystemTime<INetworkClient>.Send(this);
                await Task.Delay(TimeSyncTimeOut);
            }
        }

        public virtual string GetSession()
        {
            return Session;
        }

        public virtual void SetRecoveryData(string session, string[] recoveryKeys)
        {
            Session = session;
            RecoverySessionKeyArray = recoveryKeys;
        }
    }
}
