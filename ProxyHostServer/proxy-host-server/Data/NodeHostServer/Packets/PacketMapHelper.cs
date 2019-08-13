using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using phs.Data.NodeHostServer.Network;
using phs.Data.NodeHostServer.Info.Enums.Packets;
using System.Linq;
using System.Reflection;
using SocketServer.Utils.Buffer;
using Utils.Helper;
using Utils.Helper.Packet;

namespace phs.Data.NodeHostServer.Packets
{
    public class LobbyPacketAttribute : PacketAttribute
    {
        public LobbyPacketAttribute(ServerPacketsEnum packetId) : base((ushort)packetId)
        {
        }
    }

    public static class PacketMapHelper
    {
        /// <summary>
        /// Автоматический поиск пакетов по проект с аттрибутом LobbyPacketAttribute
        /// </summary>
        /// <param name="options"></param>
        public static void LoadPackets(this ServerOptions<NetworkClientData> options)
        {
            options.LoadPackets(typeof(LobbyPacketAttribute));
        }

        /// <summary>
        /// Добавить пакет для обработки сервером
        /// </summary>
        /// <param name="packetId">Идентификатор пакета в системе</param>
        /// <param name="packet">Обработчик пакета</param>
        /// <returns></returns>
        public static bool AddPacket(this ServerOptions<NetworkClientData> options, ServerPacketsEnum packetId, IPacket<NetworkClientData> packet)
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

        public static void SendSerialize<T>(this IClient client, ClientPacketsEnum packetId, T obj, string scheme

#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
        {
            client.SendSerialize((ushort)packetId, obj, scheme, memberName, sourceFilePath, sourceLineNumber);
        }
    }
}
