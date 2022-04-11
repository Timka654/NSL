namespace SocketCore.Utils.SystemPackets.Enums
{

    public enum ServerPacketEnum : ushort
    {
        Version = ushort.MaxValue - 3,
        RecoverySession = ushort.MaxValue - 2,
        ServerTime = ushort.MaxValue - 1,
        AliveConnection = ushort.MaxValue,
    }
}
