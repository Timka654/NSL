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
        public virtual bool AliveState => LastReceiveMessage == null || LastReceiveMessage.Value.AddMilliseconds(AliveCheckTimeOut) > DateTime.UtcNow;

        public bool GetState(bool ignoreAlive = false) => Network?.GetState() == true && (ignoreAlive || AliveState);

        public int AliveCheckTimeOut { get; set; } = 3000;

        public DateTime? DisconnectTime { get; set; }

        public ClientObjectBag ObjectBag { get; private set; }

        /// <summary>
        /// Клиент для отправки данных, эта переменная обязательна
        /// </summary>
        public IClient Network { get; set; }

        public bool ObjectBagInitialized() => ObjectBag != null;

        public void ThrowIfObjectBagNull() { if (!ObjectBagInitialized()) throw new Exception($"{nameof(ObjectBag)} not initialized"); }

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
            if (otherClient.ObjectBag == null)
                return;

            if (ObjectBag != null)
                ObjectBag.Dispose();

            ObjectBag = otherClient.ObjectBag;
            otherClient.ObjectBag = null;
        }

        /// <summary>
        /// Добавить пакет в список ожидания восстановления подключения
        /// </summary>
        /// <param name="packet_data"></param>
        /// <param name="length"></param>
        public virtual void OnPacketSendFail(byte[] packet_data, int offset, int length)
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from">copy from</param>
        public virtual void ChangeOwner(INetworkClient from)
        {
            InitializeObjectBag(from);
        }

        public virtual void Dispose()
        {
            ObjectBag?.Dispose(); 
            Network?.Disconnect();
        }
    }
}
