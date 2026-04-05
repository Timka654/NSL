namespace NSL.SocketCore.Utils
{
    /// <summary>
    /// System packet IDs reserved by NSL infrastructure.
    /// All ushort values >= <see cref="NSLSystemMinPID"/> are reserved.
    /// </summary>
    public enum NSLSystemPacketEnum : ushort
    {
        /// <summary>Default response packet ID used by <see cref="Buffer.RequestProcessor"/>.</summary>
        DefaultRequestResponse = 1,

        /// <summary>Lower boundary of the NSL-reserved PID range (ushort.MaxValue - 235 = 65300).</summary>
        NSLSystemMinPID = ushort.MaxValue - 235,

        /// <summary>UDP: client → server ping send packet (ushort.MaxValue - 101 = 65434).</summary>
        UDPSendPing = ushort.MaxValue - 101,

        /// <summary>UDP: server → client ping receive packet (ushort.MaxValue - 100 = 65435).</summary>
        UDPReceivePing = ushort.MaxValue - 100,

        /// <summary>Console engine command packet (ushort.MaxValue - 10 = 65525).</summary>
        Console = ushort.MaxValue - 10,

        /// <summary>Version negotiation packet (ushort.MaxValue - 3 = 65532).</summary>
        Version = ushort.MaxValue - 3,

        /// <summary>Session recovery packet (ushort.MaxValue - 2 = 65533).</summary>
        Session = ushort.MaxValue - 2,

        /// <summary>System time synchronisation packet (ushort.MaxValue - 1 = 65534).</summary>
        SystemTime = ushort.MaxValue - 1,

        /// <summary>Alive/keep-alive connection packet (ushort.MaxValue = 65535).</summary>
        AliveConnection = ushort.MaxValue,
    }
}
