using System;

namespace ProxyHostClient.Packets.Player.PacketData
{
    public class PlayerConnectedPacketData
    {
        public int UserId { get; set; }

        public int RoomId { get; set; }

        public Guid Id { get; set; }
    }
}
