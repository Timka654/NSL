using NSL.SocketClient.Utils;
using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.SystemPackets;
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

        public ClientOptions()
        {
            AddPacket(ClientSystemTimePacket.PacketId,
                    new ClientSystemTimePacket<TClient>(this));

            AddPacket(AliveConnectionPacket.PacketId,
                new ClientAliveConnectionPacket<TClient>(this));
        }

        /// <summary>
        /// Вызов события ошибка
        /// </summary>
        public virtual void RunException(Exception ex)
        {
            base.CallExceptionEvent(ex, ClientData);
        }

        public virtual void RunClientConnect()
        {
            base.CallClientConnectEvent(ClientData);
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
            CallClientDisconnectEvent(ClientData);
        }

        #region ServerSettings
        //Данные для настройки сервера

        #endregion

        #region Network

        public TClient ClientData { get; private set; }

        public IClient NetworkClient => ClientData.Network;

        public void InitializeClient(TClient newClientData)
        {
            if (newClientData == null)
            {
                ClientData = null;
                return;
            }

            var oldCD = ClientData;

            ClientData = newClientData;

            if (oldCD != null)
            {
                ClientData.Network = oldCD.Network;

                oldCD.Network = null;

                ClientData.ChangeOwner(oldCD);
            }
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

        public void InitializeClientObjectBagOnConnect()
        {
            this.OnClientConnectEvent += (c) =>
            {
                c.InitializeObjectBag();
            };
        }
    }
}
