using SCL.SocketClient;
using SCL.SocketClient.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SCL.SocketClient
{
    public class BaseSocketNetworkClient
    {
        /// <summary>
        /// Клиент для отправки данных, эта переменная обязательна
        /// </summary>
        public IClient Network;

        public string Session { get; set; }

        public string[] RecoverySessionKeyArray { get; set; }

        /// <summary>
        /// Буффер для хранения отправленных пакетов во время разрыва соединения
        /// </summary>
        private Queue<byte[]> WaitPacketBuffer { get; set; }
        
        public TimeSpan ServerDateTimeOffset { get; set; }

        public void Send(OutputPacketBuffer packet)
        {
            Network.Send(packet);
        }

        public DateTime? GetClientDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;
            return GetClientDateTime(dateTime.Value);
        }

        public DateTime GetClientDateTime(DateTime dateTime)
        {
            //ThreadHelper.InvokeOnMain(() => { UnityEngine.Debug.Log($"DateTime {dateTime}    {ServerDateTimeOffset}"); });
            return dateTime + ServerDateTimeOffset;
        }

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
        public IEnumerable<byte[]> GetWaitPackets()
        {
            if (WaitPacketBuffer == null || WaitPacketBuffer.Count == 0)
                return new List<byte[]>();

            return WaitPacketBuffer.ToArray();
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
    }
}
