using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    /// <summary>
    /// Mark for generate packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NSLPHGenAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packetType">Set packet type for generate send</param>
        /// <param name="models">Models for include to <see cref="NSLPHGenImplAttribute"/></param>
        public NSLPHGenAttribute(NSLPacketTypeEnum packetType, params string[] models)
        {
                
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models">Models for include to <see cref="NSLPHGenImplAttribute"/></param>
        public NSLPHGenAttribute(params string[] models)
        {
                
        }
    }
}
