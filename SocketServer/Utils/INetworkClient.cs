using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public ManualResetEvent Alive_locker { get; set; } = new ManualResetEvent(true);

        /// <summary>
        /// Клиент для отправки данных, эта переменная обязательна
        /// </summary>
        public IClient Network { get; set; }

        /// <summary>
        /// Буффер для хранения отправленных пакетов во время разрыва соединения
        /// </summary>
        private Queue<byte[]> WaitPacketBuffer { get; set; }

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
        internal void AddWaitPacket(byte[] packet_data, int lenght)
        {
            if (WaitPacketBuffer == null)
                return;
            Array.Resize(ref packet_data, lenght);
            WaitPacketBuffer.Enqueue(packet_data);
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
        public void CopyWaitPacketBuffer(INetworkClient other_client)
        {
            other_client.WaitPacketBuffer = WaitPacketBuffer;
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
                ServerTime.Send(this);
                await Task.Delay(TimeSyncTimeOut);
            }
        }
    }
}
