using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NSLPHGenImplementAttribute : Attribute
    {
        public bool IsStaticNetwork { get; set; }

        public bool DelegateOutputResponse { get; set; }

        public Type PacketsEnum { get; set; }

        public Type NetworkDataType { get; set; }

        public NSLHPDirTypeEnum Direction { get; set; }

        public NSLAccessModifierEnum Modifier { get; set; }

        public NSLPHGenImplementAttribute(params string[] models) { }
    }
}
