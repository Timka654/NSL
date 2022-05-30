using System;
using System.Collections.Generic;

namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Класс для хранения пользовательских данных
    /// </summary>
    public abstract class INetworkClient : IDisposable
    {
        public DateTime? LastReceiveMessage { get; set; }

        /// <summary>
        /// Состояние соединение на текущий момент
        /// Важно! работает только при запуске цикла сообщений PingPongEnabled
        /// </summary>
        public virtual bool AliveState { get; set; } = true;

        public bool GetState(bool ignoreAlive = false) => Network?.GetState() == true && (ignoreAlive || AliveState);

        public int AliveCheckTimeOut { get; set; } = 3000;

        public long Version { get; set; }

        public string Session { get; set; }

        public string[] RecoverySessionKeyArray { get; private set; }

        public DateTime? DisconnectTime { get; set; }

        public ClientObjectBag ObjectBag { get; private set; }

        /// <summary>
        /// Клиент для отправки данных, эта переменная обязательна
        /// </summary>
        public IClient Network { get; set; }

        /// <summary>
        /// Буффер для хранения отправленных пакетов во время разрыва соединения
        /// </summary>
        private Queue<byte[]> waitPacketBuffer;

        /// <summary>
        /// Инициализация хранилища пакетов для сохранения во время разрыва соедиенния
        /// </summary>
        public void InitializeWaitPacketBuffer()
        {
            if (waitPacketBuffer == null)
                waitPacketBuffer = new Queue<byte[]>();
        }

        /// <summary>
        /// Перенос буффера ожидающих пакетов из другого подключения
        /// </summary>
        /// <param name="other_client"></param>
        public void InitializeWaitPacketBuffer(INetworkClient otherClient)
        {
            waitPacketBuffer = otherClient.waitPacketBuffer;
            otherClient.waitPacketBuffer = null;
        }

        /// <summary>
        /// Инициализация склада объектов
        /// </summary>
        public void InitializeObjectBag()
        {
            ObjectBag = new ClientObjectBag();
        }

        /// <summary>
        /// Перенос склада объектов из другого подключения
        /// </summary>
        /// <param name="other_client"></param>
        public void InitializeObjectBag(INetworkClient otherClient)
        {
            if (ObjectBag != null)
                ObjectBag.Dispose();

            ObjectBag = otherClient.ObjectBag;
            otherClient.ObjectBag = null;
        }

        /// <summary>
        /// Добавить пакет в список ожидания восстановления подключения
        /// </summary>
        /// <param name="packet_data"></param>
        /// <param name="lenght"></param>
        public void AddWaitPacket(byte[] packet_data, int offset, int lenght)
        {
            if (waitPacketBuffer == null)
                return;

            if (offset == 0 && lenght == packet_data.Length)
                waitPacketBuffer.Enqueue(packet_data);
            else
            {
                var packet = new byte[lenght - offset];
                Array.Copy(packet_data, offset, packet, 0, lenght);
                waitPacketBuffer.Enqueue(packet);
            }

        }

        /// <summary>
        /// Получить пакет из списка ожидания
        /// </summary>
        /// <returns></returns>
        public byte[] GetWaitPacket()
        {
            if (waitPacketBuffer == null || waitPacketBuffer.Count == 0)
                return null;
            return waitPacketBuffer.Dequeue();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"> copy from</param>
        public virtual void ChangeOwner(INetworkClient from)
        {
            waitPacketBuffer = from.waitPacketBuffer;
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

        public void Dispose()
        {
            ObjectBag?.Dispose(); 
            Network?.Disconnect();
        }
    }
}
