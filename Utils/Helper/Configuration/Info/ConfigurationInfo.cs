using DBEngine;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Utils.Helper.Configuration.Info
{
    public class ConfigurationInfo
    {
        /// <summary>
        /// Полный путь(название)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Необходимо отправить клиенту
        /// </summary>
        public bool ClientValue { get; set; }

        /// <summary>
        /// Получить тип базы данных
        /// </summary>
        /// <param name="name">название базы данных(сокр.)</param>
        /// <returns></returns>
        public static DBType GetDbType(string name)
        {
            switch (name.ToLower())
            {
                case "mysql":
                    return DBType.MySql;
                case "mssql":
                    return DBType.MsSql;
                default:
                    return DBType.None;
            }
        }

        /// <summary>
        /// Получить версию ип протокола
        /// </summary>
        /// <param name="ver">Номер версии ип протокола</param>
        /// <returns></returns>
        public static AddressFamily GetIPv(byte ver)
        {
            switch (ver)
            {
                case 6:
                    return AddressFamily.InterNetworkV6;
                case 4:
                default:
                    return AddressFamily.InterNetwork;
            }
        }

        /// <summary>
        /// Получить тип протокола Network
        /// </summary>
        /// <param name="name">Название протокола</param>
        /// <returns></returns>
        public static ProtocolType GetProtocolType(string name)
        {
            switch (name.ToLower())
            {
                case "udp":
                    return ProtocolType.Udp;
                case "tcp":
                default:
                    return ProtocolType.Tcp;
            }
        }

        /// <summary>
        /// Запись конфигурации в исходящий буффер
        /// </summary>
        /// <param name="packet">Исходящий пакетный буффер</param>
        /// <param name="config">Данные конфигурации</param>
        public static void WriteConfigurationPacketData(ref OutputPacketBuffer packet, ConfigurationInfo config)
        {
            packet.WriteString16(config.Name);
            packet.WriteString16(config.Value);
        }

        public ConfigurationInfo()
        { }

        public ConfigurationInfo(string name, string value, bool clientValue = false)
        {
            Name = name;
            Value = value;
            clientValue = ClientValue;
        }
    }
}
