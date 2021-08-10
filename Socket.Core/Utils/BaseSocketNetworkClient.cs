using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;

namespace SocketCore.Utils
{
    public class BaseSocketNetworkClient : INetworkClient
    {
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
    }
}
