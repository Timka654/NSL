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

namespace phs.Data.GameServer.Packets
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeHostPacketAttribute : Attribute
    {
        public ClientPacketsEnum PacketId { get; set; }

        public NodeHostPacketAttribute(ClientPacketsEnum packetId)
        {
            PacketId = packetId;
        }
    }

    public static class PacketMapHelper
    {
        /// <summary>
        /// Автоматический поиск пакетов по проект с аттрибутом LobbyPacketAttribute
        /// </summary>
        /// <param name="options"></param>
        public static void LoadPackets(this ServerOptions<NetworkNodeHostClientData> options)
        {
            var packets = from t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                where typeof(IPacket<NetworkNodeHostClientData>).IsAssignableFrom(t)
                select t;

            NodeHostPacketAttribute attr = null;

            foreach (var item in packets)
            {
                if ((attr = (NodeHostPacketAttribute) Attribute.GetCustomAttribute(item, typeof(NodeHostPacketAttribute))) ==
                    null)
                    throw new Exception($"Packet {item.ToString()} not have RoomPacketAttribute");
                options.AddPacket((ushort) attr.PacketId,(IPacket<NetworkNodeHostClientData>)Activator.CreateInstance(item));
            }
        }

        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Идентификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public static bool AddPacket(this ServerOptions<NetworkNodeHostClientData> options, ClientPacketsEnum packetId, IPacket<NetworkNodeHostClientData> packet)
        {
            var r = options.Packets.ContainsKey((ushort)packetId);
            if (!r)
                options.Packets.Add((ushort)packetId, packet);
            return !r;
        }

        public static void SetPacketId(this OutputPacketBuffer packet, ServerPacketsEnum packetId)
        {
            packet.PacketId = (ushort)packetId;
        }
    }
}
