namespace SocketCore.Utils.SystemPackets.Enums
{
    public enum ClientPacketEnum : ushort
    {
        RecoverySessionResult = ushort.MaxValue - 2,
        ServerTimeResult = ushort.MaxValue - 1,
        AliveConnection = ushort.MaxValue
    }
}
