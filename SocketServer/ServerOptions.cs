using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketCore.Utils.SystemPackets.Enums;
using SocketServer.Utils;
using SocketServer.Utils.SystemPackets;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Пакеты которые будет принимать и обрабатывать сервер
        /// </summary>
        public Dictionary<ushort, IPacket<T>> Packets = new Dictionary<ushort, IPacket<T>>()
        {
            { (ushort)ClientPacketEnum.AliveConnection, new AliveConnection<T>() },
            { (ushort)ClientPacketEnum.RecoverySession, new RecoverySession<T>() },
            { (ushort)ClientPacketEnum.Version, new Version<T>() },
        };

        public Dictionary<ushort, PacketHandle> PacketHandles = new Dictionary<ushort, PacketHandle>()
        {

        };

        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Индификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public override bool AddPacket(ushort packetId, IPacket<T> packet)
        {
            var r = Packets.ContainsKey(packetId) || PacketHandles.ContainsKey(packetId);
            if (!r)
            {
                Packets.Add(packetId, packet);
                PacketHandles.Add(packetId, packet.Receive);
            }
            return !r;
        }

        public override IPacket<T> GetPacket(ushort packetId)
        {
            Packets.TryGetValue(packetId, out var result);
            return result;
        }

        public event OnRecoverySessionReceiveDelegate<T> OnRecoverySessionReceiveEvent
        {
            add { RecoverySession<T>.Instance.OnRecoverySessionReceiveEvent += value; }
            remove { RecoverySession<T>.Instance.OnRecoverySessionReceiveEvent -= value; }
        }

        public ServerOptions()
        {
            PacketHandles = Packets.ToDictionary(x => x.Key, x => (PacketHandle)x.Value.Receive);
        }
    }
}
