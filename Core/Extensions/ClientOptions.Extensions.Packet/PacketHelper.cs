using NSL.SocketClient;
using System;
using System.Reflection;

namespace NSL.ClientOptions.Extensions.Packet
{
    public static class PacketHelper
    {
        /// <summary>
        /// Инициализация пакетов (классов реализующих интерфейс <see cref="IPacket{T}"/>) по аттрибуту наследуемому от аттрибута <see cref="Packet.PacketAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы пакетов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы пакетов</param>
        /// <returns>Кол-во пакетов которые были инициализированы</returns>
        public static int LoadPackets<T>(this ClientOptions<T> clientOptions, Assembly assembly, Type selectAttrbuteType)
            where T : BaseSocketNetworkClient
        {
            return SocketCore.Extensions.Packet.PacketHelper.LoadPackets(clientOptions, assembly, selectAttrbuteType, (type) => Activator.CreateInstance(type, clientOptions) as IPacket<T>);
        }

        public static int LoadPackets<T>(this ClientOptions<T> clientOptions, Type selectAttrbuteType) 
            where T : BaseSocketNetworkClient
        {
            return LoadPackets<T>(clientOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }

    }
}