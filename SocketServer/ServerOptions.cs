using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using SocketServer.Utils;
using SocketServer.Utils.SystemPackets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketServer
{
    public class ServerOptions<T> : CoreOptions<T> where T : IServerNetworkClient
    {
        #region ServerSettings
        //Данные для настройки сервера

        /// <summary>
        /// Длина очереди для приема подключения
        /// </summary>
        public int Backlog { get; set; }

        #endregion

        /// <summary>
        /// Делегат для регистрации пакета
        /// </summary>
        /// <param name="client">Данные клиента</param>
        /// <param name="data">Входящий буффер с данными</param>
        /// <param name="output">Исходящий буффер с данными(не обязательно)</param>
        public delegate void PacketHandle(T client, InputPacketBuffer data);

        /// <summary>
        /// Делегат для регистрации события перехвата сетевых ошибок
        /// </summary>
        /// <param name="ex">Возникшая ошибка</param>
        /// <param name="s">Сокет с которым произошла ошибка</param>
        public delegate void ExtensionHandle(Exception ex, T client);

        /// <summary>
        /// Делегат для регистрации события перехвата подключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientConnect(T client);

        /// <summary>
        /// Делегат для регистрации события перехвата отключения клиента
        /// </summary>
        /// <param name="client">Данные клиента</param>
        public delegate void ClientDisconnect(T client);

        /// <summary>
        /// События вызываемое при получении ошибки
        /// </summary>
        public event ExtensionHandle OnExtensionEvent;

        /// <summary>
        /// Событие вызываемое при подключении клиента
        /// </summary>
        public event ClientConnect OnClientConnectEvent;

        /// <summary>
        /// Событие вызываемое при отключении клиента
        /// </summary>
        public event ClientDisconnect OnClientDisconnectEvent;

        /// <summary>
        /// Вызов события ошибка
        /// </summary>
        public void RunExtension(Exception ex, T client)
        {
            OnExtensionEvent?.Invoke(ex, client);
        }

        /// <summary>
        /// Вызов события подключения клиента
        /// </summary>
        /// <param name="client"></param>
        public void RunClientConnect(T client)
        {
            OnClientConnectEvent?.Invoke(client);
        }

        /// <summary>
        /// Вызов события отключения клиента
        /// </summary>
        /// <param name="client"></param>
        public void RunClientDisconnect(T client)
        {
            OnClientDisconnectEvent?.Invoke(client);
        }

        /// <summary>
        /// Пакеты которые будет принимать и обрабатывать сервер
        /// </summary>
        public Dictionary<ushort, IPacket<T>> Packets = new Dictionary<ushort, IPacket<T>>()
        {
            { (ushort)ClientPacketEnum.AliveConnection, new AliveConnection<T>() },
            { (ushort)ClientPacketEnum.RecoverySession, new RecoverySession<T>() },
            { (ushort)ClientPacketEnum.Version, new Version<T>() },
        };

        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Индификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public override bool AddPacket(ushort packetId, IPacket<T> packet)
        {
            var r = Packets.ContainsKey(packetId);
            if (!r)
                Packets.Add(packetId, packet);
            return !r;
        }

        public IPacket<T> GetPacket(ushort packetId)
        {
            Packets.TryGetValue(packetId, out var result);
            return result;
        }

        public event OnRecoverySessionReceiveDelegate<T> OnRecoverySessionReceiveEvent
        {
            add { RecoverySession<T>.Instance.OnRecoverySessionReceiveEvent += value; }
            remove { RecoverySession<T>.Instance.OnRecoverySessionReceiveEvent -= value; }
        }
    }
}
