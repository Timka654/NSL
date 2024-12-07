using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NSLPHGenImplementAttribute : Attribute
    {
        public NSLPHGenImplementAttribute(Type packetsEnum, Type networkDataType, HPDirTypeEnum direction, params string[] models) { }

        public NSLPHGenImplementAttribute(Type packetsEnum, Type networkDataType, HPDirTypeEnum direction, AccessModifierEnum modifier, params string[] models) { }
    }
}
