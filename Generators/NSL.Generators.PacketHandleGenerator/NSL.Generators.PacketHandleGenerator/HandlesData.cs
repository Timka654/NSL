using Microsoft.CodeAnalysis;
using NSL.Generators.PacketHandleGenerator.Shared;
using System;
using System.Linq;

namespace NSL.Generators.PacketHandleGenerator
{
    internal class HandlesData
    {
        public GeneratorExecutionContext Context { get; set; }

        public ITypeSymbol Type { get; set; }

        public AccessModifierEnum Modifiers { get; set; }

        public HPDirTypeEnum Direction { get; set; }

        public ITypeSymbol NetworkDataType { get; set; }

        public string[] Models { get; set; }

        public PacketData[] Packets { get; set; }

        public string BuildModifiers()
            => string.Join(" ", Enum.GetValues(typeof(AccessModifierEnum))
            .Cast<AccessModifierEnum>()
            .Where(x => Modifiers.HasFlag(x))
            .Select(x => x.ToString().ToLower()));

        public string BuildModifierForHandles(AccessModifierEnum mod = AccessModifierEnum.Private)
            => string.Join(" ", Enumerable.Repeat(mod, 1).Concat(Enum.GetValues(typeof(AccessModifierEnum))
                .Cast<AccessModifierEnum>()
                .Where(x => x == AccessModifierEnum.Static)
                .Where(x => Modifiers.HasFlag(x)))
                .Select(x => x.ToString().ToLower()));
    }
}
