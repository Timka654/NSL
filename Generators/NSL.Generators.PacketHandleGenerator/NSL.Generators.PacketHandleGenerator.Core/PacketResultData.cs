using Microsoft.CodeAnalysis;

namespace NSL.Generators.PacketHandleGenerator.Core
{
    internal class PacketResultData
    {
        public ITypeSymbol Type { get; set; }

        public string BinaryModel { get; set; }
    }
}
