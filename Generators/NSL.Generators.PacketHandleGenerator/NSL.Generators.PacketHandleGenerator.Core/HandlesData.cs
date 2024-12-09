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

        public NSLAccessModifierEnum Modifiers { get; set; }

        public NSLHPDirTypeEnum Direction { get; set; }

        public ITypeSymbol NetworkDataType { get; set; }

        public string[] Models { get; set; }

        public bool IsStaticNetwork { get; set; }

        public bool DelegateOutputResponse { get; set; }

        public PacketData[] Packets { get; set; }

        public Func<string, bool> HaveReceiveHandleImplementation { get; set; } = (name) => false;

        public string BuildModifiers()
            => string.Join(" ", Enum.GetValues(typeof(NSLAccessModifierEnum))
            .Cast<NSLAccessModifierEnum>()
            .Where(x => Modifiers.HasFlag(x))
            .Select(x => x.ToString().ToLower()));

        public string BuildModifierForHandles(NSLAccessModifierEnum mod = NSLAccessModifierEnum.Private)
            => string.Join(" ", Enumerable.Repeat(mod, 1).Concat(Enum.GetValues(typeof(NSLAccessModifierEnum))
                .Cast<NSLAccessModifierEnum>()
                .Where(x => x == NSLAccessModifierEnum.Static)
                .Where(x => Modifiers.HasFlag(x)))
                .Select(x => x.ToString().ToLower()));
    }
}
