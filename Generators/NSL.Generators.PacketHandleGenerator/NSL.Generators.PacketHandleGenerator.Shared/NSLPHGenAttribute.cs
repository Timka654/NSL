using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NSLPHGenAttribute : Attribute
    {
        public NSLPHGenAttribute(NSLPacketTypeEnum packetType, params string[] models)
        {
                
        }

        public NSLPHGenAttribute(params string[] models)
        {
                
        }
    }
}
