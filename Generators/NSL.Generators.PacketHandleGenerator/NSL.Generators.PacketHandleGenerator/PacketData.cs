using Microsoft.CodeAnalysis;
using NSL.Generators.PacketHandleGenerator.Shared;

namespace NSL.Generators.PacketHandleGenerator
{
    internal class PacketData
    {
        public HandlesData HandlesData { get; set; }

        public string Name { get; set; }

        public PacketTypeEnum PacketType { get; set; }

        public string[] Models { get; set; }

        public PacketParamData[] Parameters { get; set; }

        public PacketResultData Result { get; set; }
        public IFieldSymbol EnumMember { get; internal set; }
    }
}
