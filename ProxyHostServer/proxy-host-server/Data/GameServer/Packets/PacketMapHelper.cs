using phs.Data.GameServer.Info.Enums.Packets;
using phs.Data.GameServer.Network;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using phs.Data.NodeHostServer.Packets;
using SocketServer.Utils.Buffer;
using Utils.Helper.Packet;

namespace phs.Data.GameServer.Packets
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GamePacketAttribute : PacketAttribute
    {
        public GamePacketAttribute(ServerPacketsEnum packetId) : base((ushort)packetId)
        {
        }
    }

    public static class PacketMapHelper
    {
        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Идентификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public static bool AddPacket(this ServerOptions<NetworkGameServerData> options, ServerPacketsEnum packetId, IPacket<NetworkGameServerData> packet)
        {
            var r = options.Packets.ContainsKey((ushort)packetId);
            if (!r)
                options.Packets.Add((ushort)packetId, packet);
            return !r;
        }

        public static void SetPacketId(this OutputPacketBuffer packet, ClientPacketsEnum packetId)
        {
            packet.PacketId = (ushort)packetId;
        }
    }
}
