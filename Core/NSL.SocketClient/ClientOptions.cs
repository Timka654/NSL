using NSL.SocketClient.Utils;
using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.SystemPackets.Enums;
using System;
using System.Net;

namespace NSL.SocketClient
{
    public class ClientOptions<TClient> : CoreOptions<TClient>
        where TClient : BaseSocketNetworkClient
    {
        #region EventDelegates

        /// <summary>
        /// Делегат для регистрации события перехвата сетевых ошибок
        /// </summary>
        /// <param name="ex">Возникшая ошибка</param>
        /// <param name="s">Сокет с которым произошла ошибка</param>
        public delegate void ExtensionHandleDelegate(Exception ex, TClient client);

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
        public delegate void ClientConnectedDelegate(TClient client);

        /// <summary>
        /// Делегат для регистрации события перехвата отключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientDisconnectedDelegate(TClient client);

        #endregion

        #region Events

        public event ReconnectDelegate OnReconnectEvent;

        public event Utils.SystemPackets.RecoverySessionPacket<TClient>.OnReceiveEventHandle OnRecoverySessionEvent;

        #endregion

        public ClientOptions()
        {
            var recoverySession = new Utils.SystemPackets.RecoverySessionPacket<TClient>(this);

            recoverySession.OnReceiveEvent += RunRecoverySession;

            AddPacket((ushort)ClientPacketEnum.ServerTimeResult,
                    new Utils.SystemPackets.ClientSystemTimePacket<TClient>(this));

            AddPacket((ushort)ClientPacketEnum.AliveConnection,
                new Utils.SystemPackets.ClientAliveConnectionPacket<TClient>(this));

            AddPacket((ushort)ServerPacketEnum.RecoverySession,
                recoverySession);
        }

        /// <summary>
        /// Вызов события ошибка
        /// </summary>
        public virtual void RunException(Exception ex)
        {
            base.RunException(ex, ClientData);
        }

        public virtual void RunClientConnect()
        {
            base.RunClientConnect(ClientData);
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

            OnRunClientDisconnect();

            //if (EnableAutoRecovery)
            //    OldSessionClientData?.RunRecovery();
        }

        protected virtual void OnRunClientDisconnect()
        {
            RunClientDisconnect(ClientData);
        }

        internal void RunRecoverySession(RecoverySessionResultEnum result)
        {
            OnRecoverySessionEvent?.Invoke(result);
        }

        #region Recovery

        public bool EnableAutoRecovery { get; set; }

        public int MaxRecoveryTryTime { get; set; } = 5;

        public int RecoveryWaitTime { get; set; } = 1900;


        #endregion

        #region ServerSettings
        //Данные для настройки сервера

        #endregion

        #region Network

        public TClient ClientData { get; private set; }

        public IClient NetworkClient => ClientData.Network;

        public void InitializeClient(TClient setClient)
        {
            ClientData = setClient;
            ClientData.Network = NetworkClient;
        }

        /// <summary>
        /// Ип адресс - используется для подключения к случателю
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Порт - используется для подключению к слушателю (по умолчанию - должен соответствовать порту слушателя)
        /// </summary>
        public int Port { get; set; }

        public IPAddress GetIPAddress() => IPAddress.Parse(IpAddress);

        public IPEndPoint GetIPEndPoint() => new IPEndPoint(GetIPAddress(), Port);

        #endregion

        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Индификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public bool AddPacket(ushort packetId, IClientPacket<TClient> packet)
        {
            return AddPacket(packetId, (IPacket<TClient>)packet);
        }

        public void ClearRecoveryData()
        {
            if (ClientData != null)
            {
                ClientData.SetRecoveryData(null, null);
            }
        }
    }
}
