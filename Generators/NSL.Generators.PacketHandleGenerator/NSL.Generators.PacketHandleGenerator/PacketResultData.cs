using Microsoft.CodeAnalysis;

namespace NSL.Generators.PacketHandleGenerator
{
    internal class PacketResultData
    {
        public ITypeSymbol Type { get; set; }

        public string BinaryModel { get; set; }
    }
}
