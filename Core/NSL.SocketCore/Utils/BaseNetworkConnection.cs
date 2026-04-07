#if NSL_LIBRARY
using Microsoft.Extensions.DependencyInjection;
#endif
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.SystemPackets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// Класс для хранения пользовательских данных
    /// </summary>
    public abstract class BaseNetworkConnection : IDisposable
    {
        public DateTime? LastReceiveMessage { get; set; }

        /// <summary>
        /// Состояние соединение на текущий момент
        /// Важно! работает только при запуске цикла сообщений PingPongEnabled
        /// </summary>
        public virtual bool AliveState => LastReceiveMessage == null || LastReceiveMessage.Value.AddMilliseconds(AliveCheckTimeOut) > DateTime.UtcNow;

        public bool GetState(bool ignoreAlive = false) => Network?.GetState() == true && (ignoreAlive || AliveState);

        public int AliveCheckTimeOut { get; set; } = 3000;

        #region PingPong

        private bool _pingPongEnabled;
        private CancellationTokenSource _pingPongCts;
        private volatile int _pingPending;
        private DateTime _pingRequestTime;

        public bool PingPongEnabled
        {
            get => _pingPongEnabled;
            set
            {
                if (value == _pingPongEnabled) return;
                _pingPongEnabled = value;
                _pingPongCts?.Cancel();
                _pingPongCts = null;
                if (_pingPongEnabled)
                {
                    _pingPongCts = new CancellationTokenSource();
                    RunAliveChecker(_pingPongCts.Token);
                }
                else
                {
                    Ping = 0;
                }
            }
        }

        public int Ping { get; protected set; }

        public bool IsPingPending => _pingPending != 0;

        public void RequestPing()
        {
            if (Interlocked.CompareExchange(ref _pingPending, 1, 0) == 0 && Network != null)
            {
                _pingRequestTime = DateTime.UtcNow;
                Network.SendEmpty(AliveConnectionPacket.PacketId);
            }
        }

        public void PongProcess()
        {
            Ping = (int)((DateTime.UtcNow - _pingRequestTime).TotalMilliseconds / 2.0);
            Interlocked.Exchange(ref _pingPending, 0);
        }

        private async void RunAliveChecker(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && Network?.GetState() == true)
                {
                    RequestPing();
                    await Task.Delay(AliveCheckTimeOut / 2, token);
                }
            }
            catch (OperationCanceledException) { }
        }

        #endregion

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

        /// <summary>
        /// Ссылка на параметры, с которыми поднято подключение.
        /// Эквивалентно <see cref="Network"/>?.Options, но также допускает явную установку
        /// до того, как <see cref="Network"/> будет назначен (например на серверной стороне).
        /// </summary>
        public CoreOptions Options
        {
            get => _options ?? Network?.Options;
            set => _options = value;
        }

        private CoreOptions _options;

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
        public void InitializeObjectBag(BaseNetworkConnection otherClient)
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
        public virtual void ChangeOwner(BaseNetworkConnection from)
        {
            //InitializeObjectBag(from);
        }

        public virtual void Dispose()
        {
            PingPongEnabled = false;
            ObjectBag?.Dispose();
#if NSL_LIBRARY
            _serviceScope?.Dispose();
#endif
            Network?.Disconnect();
        }
    }
}