using System;

namespace NSL.SocketCore.Utils.Packet.FastEvent
{
    public class FastEventPacketAttribute : Attribute
    {
        public Type Type { get; }
        public bool Large { get; }

        /// <param name="type">Method receive type</param>
        /// <param name="large">If <see langword="true"/> — int32 length (up to <see cref="int.MaxValue"/>), otherwise int16.</param>
        public FastEventPacketAttribute(Type type = null, bool large = false)
        {
            Type = type;
            Large = large;
        }
    }
}
