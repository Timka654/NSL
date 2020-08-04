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
    public abstract class CoreOptions<T> : CoreOptions
        where T : INetworkClient
    {
        public abstract bool AddPacket(ushort packetId, IPacket<T> packet);
    }
}
