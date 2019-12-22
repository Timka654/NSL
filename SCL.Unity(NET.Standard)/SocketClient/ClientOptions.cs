using SCL.SocketClient.Utils;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketServer.Utils.SystemPackets.Enums;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SCL.SocketClient
{
    public class ClientOptions<T>
        where T : BaseSocketNetworkClient
    {
        #region EventDelegates

        /// <summary>
        /// Делегат для регистрации пакета
        /// </summary>
        /// <param name="client">Данные клиента</param>
        /// <param name="data">Входящий буффер с данными</param>
        /// <param name="output">Исходящий буффер с данными(не обязательно)</param>
        public delegate void PacketHandle(InputPacketBuffer data);

        /// <summary>
        /// Делегат для регистрации события перехвата сетевых ошибок
        /// </summary>
        /// <param name="ex">Возникшая ошибка</param>
        /// <param name="s">Сокет с которым произошла ошибка</param>
        public delegate void ExtensionHandleDelegate(Exception ex, T client);

        /// <summary>
        /// Делегат для регистрации события уведомлении о новой попытке переподключится
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="client"></param>
        public delegate void ReconnectDelegate(int currentTry, bool result);

        /// <summary>
        /// Делегат для регистрации события перехвата подключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientConnectedDelegate(T client);

        /// <summary>
        /// Делегат для регистрации события перехвата отключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientDisconnectedDelegate(T client);

        #endregion

        #region Events

        /// <summary>
        /// События вызываемое при получении ошибки
        /// </summary>
        public event ExtensionHandleDelegate OnExtensionEvent;

        /// <summary>
        /// Событие вызываемое при подключении клиента
        /// </summary>
        public event ClientConnectedDelegate OnClientConnectEvent;

        /// <summary>
        /// Событие вызываемое при отключении клиента
        /// </summary>
        public event ClientDisconnectedDelegate OnClientDisconnectEvent;

        public event ReconnectDelegate OnReconnectEvent;

        public event Utils.SystemPackets.RecoverySession<T>.OnReceiveEventHandle OnRecoverySessionEvent;

        #endregion

        public ClientOptions()
        {
            var recoverySession = new Utils.SystemPackets.RecoverySession<T>(this);

            recoverySession.OnReceiveEvent += RunRecoverySession;

            AddPacket((ushort)ServerPacketEnum.SystemTime,
                    new Utils.SystemPackets.SystemTime<T>(this));

            AddPacket((ushort)ServerPacketEnum.AliveConnection,
                new Utils.SystemPackets.AliveConnection<T>(this));

            AddPacket((ushort)ServerPacketEnum.RecoverySessionResult,
                recoverySession);
        }

        /// <summary>
        /// Вызов события ошибка
        /// </summary>
        public void RunExtension(Exception ex)
        {
            ThreadHelper.InvokeOnMain(() => { OnExtensionEvent?.Invoke(ex, ClientData); });
        }

        /// <summary>
        /// Вызов события подключения клиента
        /// </summary>
        /// <param name="client"></param>
        public void RunClientConnect()
        {
            ThreadHelper.InvokeOnMain(() => { OnClientConnectEvent?.Invoke(ClientData); });
        }

        /// <summary>
        /// Вызов события отключения клиента
        /// </summary>
        /// <param name="client"></param>
        public void RunClientDisconnect()
        {
            foreach (var packet in Packets.Values)
            {
                var lockedPacket = packet as ILockedPacket;
                lockedPacket?.UnlockPacket();
            }

            ThreadHelper.InvokeOnMain(() => { OnClientDisconnectEvent?.Invoke(ClientData); });

            if (EnableAutoRecovery)
                RunRecovery();
        }

        internal void RunRecoverySession(RecoverySessionResultEnum result)
        {
            OnRecoverySessionEvent?.Invoke(result);
        }

        #region Recovery

        public bool EnableAutoRecovery { get; set; }

        public int MaxRecoveryTryTime { get; set; } = 5;

        public int RecoveryWaitTime { get; set; } = 1900;

        public async void RunRecovery()
        {
            if (NetworkClient == null || ClientData == null)
            {
                OnReconnectEvent?.Invoke(MaxRecoveryTryTime, false);
                return;
            }

            for (int currentTry = 0; currentTry < MaxRecoveryTryTime && NetworkClient != null; currentTry++)
            {
                var result = await NetworkClient.ConnectAsync();

                OnReconnectEvent?.Invoke(currentTry + 1, result);

                if (result)
                    break;

                await Task.Delay(RecoveryWaitTime);
            }

            byte[] buffer;
            while ((buffer = ClientData.GetWaitPacket()) != null)
            {
                NetworkClient.Send(buffer,0,buffer.Length);
            }
        }

        #endregion

        #region ServerSettings
        //Данные для настройки сервера

        /// <summary>
        /// Тип ип адресса, InterNetwork - IPv4, InterNetworkV6 - IPv6
        /// </summary>
        public AddressFamily AddressFamily { get; set; }

        /// <summary>
        /// Тип сервера, обычно используется Tcp/Udp
        /// </summary>
        public ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// Ип для инициализации сервера на определенном адаптере (0.0.0.0 - на всех, стандартное значение)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Порт для инициализации сервера 
        /// </summary>
        public int Port { get; set; }

        #endregion

        #region Network

        public T ClientData { get; set; }

        public SocketClient<T> NetworkClient { get; set; }

        /// <summary>
        /// Размер буффера приходящих данных, если пакет больше этого значения то данные по реализованному алгоритму принять не получиться
        /// </summary>
        public int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Алгоритм шифрования входящих пакетов
        /// </summary>
        public IPacketCipher inputCipher { get; set; }

        /// <summary>
        /// Алгоритм шифрования входящих пакетов
        /// </summary>
        public IPacketCipher outputCipher { get; set; }

        /// <summary>
        /// Пакеты которые будет принимать и обрабатывать сервер
        /// </summary>
        public Dictionary<ushort, PacketHandle> PacketHandles = new Dictionary<ushort, PacketHandle>();

        public Dictionary<ushort, IPacket<T>> Packets = new Dictionary<ushort, IPacket<T>>();

        #endregion

        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Индификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public bool AddPacket(ushort packetId, IPacket<T> packet)
        {
            var r = PacketHandles.ContainsKey(packetId);
            if (!r)
            {
                Packets.Add(packetId, packet);
                PacketHandles.Add(packetId, packet.GetReceiveHandle());
            }
            return !r;
        }

        public void ClearRecoveryData()
        {
            if (ClientData != null)
            {
                ClientData.Session = null;
                ClientData.RecoverySessionKeyArray = null;
            }
        }

    }
}
