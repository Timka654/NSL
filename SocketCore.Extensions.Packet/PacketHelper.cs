using SocketCore;
using SocketCore.Utils;
using SocketServer.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ServerOptions.Extensions.Packet
{
    public static class PacketHelper
    {
        /// <summary>
        /// Инициализация пакетов (классов реализующих интерфейс <see cref="IPacket{TClient}"/>) по аттрибуту наследуемому от аттрибута <see cref="Packet.PacketAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы пакетов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы пакетов</param>
        /// <returns>Кол-во пакетов которые были инициализированы</returns>
        public static int LoadPackets<T>(this CoreOptions<T> serverOptions, Assembly assembly, Type selectAttrbuteType) where T : INetworkClient
        {
            if (!typeof(Packet.PacketAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Packet.PacketAttribute).FullName}");
            }

            var types = assembly
                .GetTypes()
                .Select(x => new
                {
                    type = x,
                    attr = (Packet.PacketAttribute)x.GetCustomAttribute(selectAttrbuteType)
                })
                .Where(x => x.attr != null);

            bool server = typeof(IServerNetworkClient).IsAssignableFrom(typeof(T));



            foreach (var item in types)
            {
                Debug.WriteLine($"Loading Packet: packet: {item.attr.PacketId} type: {item.type.FullName}");

                if (!typeof(IPacket<T>).IsAssignableFrom(item.type))
                    throw new Exception($"Packet type {typeof(IPacket<T>)} is not assignable from {item.type}");

                bool r = false;

                if (server)
                    r = serverOptions.AddPacket((ushort)item.attr.PacketId, (IPacket<T>)Activator.CreateInstance(item.type));
                else
                    r = serverOptions.AddPacket((ushort)item.attr.PacketId, (IPacket<T>)Activator.CreateInstance(item.type,serverOptions));

                Debug.WriteLine($"Loading Packet: packet: {item.attr.PacketId} type: {item.type.FullName} result: {r}");
            }

            return types.Count();
        }

        /// <summary>
        /// Инициализация пакетов (классов реализующих интерфейс <see cref="IPacket{TClient}"/>) по аттрибуту наследуемому от аттрибута <see cref="Packet.PacketAttribute"/> из сборки с которой был произведен вызов функции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы пакетов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы пакетов</param>
        /// <returns>Кол-во пакетов которые были инициализированы</returns>
        //public static int LoadPackets<T>(this CoreOptions serverOptions, Type selectAttrbuteType) where T : IServerNetworkClient
        //{
        //    return LoadPackets<T>(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        //}

        public static int LoadPackets<T>(this CoreOptions<T> serverOptions, Type selectAttrbuteType) where T : INetworkClient
        {
            return LoadPackets<T>(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }

    }
}
