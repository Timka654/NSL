#if NSL_LIBRARY
using Microsoft.Extensions.DependencyInjection;
#endif
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Threading;

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

#if NSL_LIBRARY

        private IServiceScope _serviceScope;

        /// <summary>
        /// Scoped DI-контейнер клиента. Равен <c>null</c> до явного вызова <see cref="InitializeServiceScope"/>.
        /// Как правило инициализируется после успешной авторизации.
        /// </summary>
        public IServiceScope ServiceScope => _serviceScope;

        public bool ServiceScopeInitialized() => _serviceScope != null;

        /// <summary>
        /// Инициализирует Scoped DI-контейнер. Безопасно для параллельных вызовов: первый вызов выигрывает, остальные игнорируются.
        /// </summary>
        /// <returns><c>true</c> если scope был создан, <c>false</c> если уже был инициализирован.</returns>
        public bool InitializeServiceScope(IServiceProvider provider)
        {
            var newScope = provider.CreateScope();
            if (Interlocked.CompareExchange(ref _serviceScope, newScope, null) != null)
            {
                newScope.Dispose();
                return false;
            }
            return true;
        }

#endif

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
            if (ObjectBag == default)
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

        public virtual void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            var _network = Network;

            _network?.Send(packet, disposeOnSend);

            if (_network == null)
            {
                var buf = packet.CompilePacket();

                if (disposeOnSend) packet.Dispose();

                OnPacketSendFail(buf, 0, buf.Length);
            }
        }

        public virtual void Send(byte[] buf, int offset, int len)
        {
            var _network = Network;

            _network?.Send(buf, offset, len);

            if (_network == null)
            {
                OnPacketSendFail(buf, offset, len);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from">copy from</param>
        public virtual void ChangeOwner(INetworkClient from)
        {
            //InitializeObjectBag(from);
        }

        public virtual void Dispose()
        {
            ObjectBag?.Dispose();
#if NSL_LIBRARY
            _serviceScope?.Dispose();
#endif
            Network?.Disconnect();
        }
    }
}