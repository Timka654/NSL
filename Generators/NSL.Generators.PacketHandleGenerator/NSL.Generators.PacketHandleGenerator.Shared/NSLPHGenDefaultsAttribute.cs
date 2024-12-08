using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class NSLPHGenDefaultsAttribute : Attribute
    {
        public NSLPHGenDefaultsAttribute(NSLAccessModifierEnum defaultModifiers = 0, bool defaultAsync = false)
        {
                
        }
    }
}
