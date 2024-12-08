using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NSLPHGenImplementAttribute : Attribute
    {
        public bool IsStaticNetwork { get; set; }

        public NSLPHGenImplementAttribute(Type packetsEnum, Type networkDataType, NSLHPDirTypeEnum direction, params string[] models) { }

        public NSLPHGenImplementAttribute(Type packetsEnum, Type networkDataType, NSLHPDirTypeEnum direction, NSLAccessModifierEnum modifier, params string[] models) { }
    }
}
