namespace NSL.UDP.Enums
{
    public enum UDPChannelEnum : byte
    {
		/// <summary>
		/// Must be combined with <see cref="Ordered"/> or <see cref="Unordered"/>
		/// </summary>
		Reliable = 1,
		/// <summary>
		/// Must be combined with <see cref="Ordered"/> or <see cref="Unordered"/>
		/// </summary>
		Unreliable = 2,

		/// <summary>
		/// Must be combined with <see cref="Reliable"/> or <see cref="Unreliable"/>
		/// </summary>
		Ordered = 4,
		/// <summary>
		/// Must be combined with <see cref="Reliable"/> or <see cref="Unreliable"/>
		/// </summary>
		Unordered = 8,
        ReliableOrdered = Reliable | Ordered,
        ReliableUnordered = Reliable | Unordered,
        UnreliableOrdered = Unreliable | Ordered,
        UnreliableUnordered = Unreliable | Unordered,
    }
}
