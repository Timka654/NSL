using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class BinaryTypeIOGeneratorContext : BinaryGeneratorContext
    {
        public string For { get; set; } = "*";

        public override bool IsIgnore(ISymbol symbol, string path)
        {
            if (HasIgnore(symbol))
                return true;

            if (For.Equals("*"))
                return false;

            return symbol.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(BinaryIODataAttribute))
                .Where(x => x.NamedArguments.Any(b => b.Key == "For" && For.Equals((string)b.Value.Value)))
                .Any() == false;
        }

        private bool HasIgnore(ISymbol symbol)
            => symbol.GetAttributes()
                    .Where(x => x.AttributeClass.Name == nameof(BinaryIODataIgnoreAttribute))
                    .Where(x => x.NamedArguments.Any() || x.NamedArguments.Any(b => b.Key == "For" && For.Equals((string)b.Value.Value)))
                    .Any();
    }
}
