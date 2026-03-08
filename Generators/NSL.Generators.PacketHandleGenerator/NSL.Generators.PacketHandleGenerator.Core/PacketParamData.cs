using Microsoft.CodeAnalysis;

namespace NSL.Generators.PacketHandleGenerator.Core
{
    internal class PacketParamData
    {
        public ITypeSymbol Type { get; set; }

        public string BinaryModel { get; set; }

        public string Name { get; set; }
        public AttributeData Attribute { get; internal set; }
    }
}
