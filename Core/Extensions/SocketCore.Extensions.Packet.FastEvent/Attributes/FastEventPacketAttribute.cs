using System;

namespace NSL.SocketCore.Extensions.Packet.FastEvent.Attributes
{
    public class FastEventPacketAttribute : Attribute
    {
        public Type Type { get; }

        public bool Large { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Method receive type</param>
        /// <param name="large">if large equals <see cref="true"/> FastEvent processing as int32 len(<see cref="int.MaxValue"/>),also <see cref="false"/> FastEvent receive <see cref="short.MaxValue"/></param>
        public FastEventPacketAttribute(Type type = null, bool large = false)
        {
            this.Type = type;
            this.Large = large;
        }
    }
}
