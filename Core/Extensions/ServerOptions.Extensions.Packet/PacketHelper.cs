using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Reflection;

namespace NSL.ServerOptions.Extensions.Packet
{
    public static class PacketHelper
    {
        /// <summary>
        /// Инициализация пакетов (классов реализующих интерфейс <see cref="IPacket{T}"/>) по аттрибуту наследуемому от аттрибута <see cref="Packet.PacketAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы пакетов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы пакетов</param>
        /// <returns>Кол-во пакетов которые были инициализированы</returns>
        public static int LoadPackets<T>(this ServerOptions<T> serverOptions, Assembly assembly, Type selectAttrbuteType)
            where T : IServerNetworkClient
        {
            return SocketCore.Extensions.Packet.PacketHelper.LoadPackets(serverOptions, assembly, selectAttrbuteType, (type) => Activator.CreateInstance(type) as IPacket<T>);
        }

        public static int LoadPackets<T>(this ServerOptions<T> serverOptions, Type selectAttrbuteType) 
            where T : IServerNetworkClient
        {
            return LoadPackets<T>(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }

    }
}