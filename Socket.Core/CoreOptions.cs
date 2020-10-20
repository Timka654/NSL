using SocketCore.Utils;
using SocketCore.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketCore
{
    public class CoreOptions
    {
        public IBasicLogger HelperLogger { get; set; }

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
    }

    /// <summary>
    /// Содержит функции для которых необходимо явное указывание типа клиента наследуется <see cref="CoreOptions"/>
    /// </summary>
    /// <typeparam name="T">INetworkClient</typeparam>
    public class CoreOptions<T> : CoreOptions
        where T : INetworkClient
    {
        /// <summary>
        /// Делегат для регистрации события перехвата сетевых ошибок
        /// </summary>
        /// <param name="ex">Возникшая ошибка</param>
        /// <param name="s">Сокет с которым произошла ошибка</param>
        public delegate void ExceptionHandle(Exception ex, T client);

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
        public event ExceptionHandle OnExceptionEvent;

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
        public void RunException(Exception ex, T client)
        {
            OnExceptionEvent?.Invoke(ex, client);
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

        public virtual bool AddPacket(ushort packetId, IPacket<T> packet) { throw new NotImplementedException(); }
    }
}
