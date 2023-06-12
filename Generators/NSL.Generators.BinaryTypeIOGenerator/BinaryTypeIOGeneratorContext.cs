using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class BinaryTypeIOGeneratorContext : BinaryGeneratorContext
    {
        public string For { get; set; } = "*";

        public override bool IsIgnore(ISymbol symbol)
        {
            if (For.Equals("*"))
                return false;

            return symbol.GetAttributes()
                .Where(x => x.AttributeClass.Name == "BinaryIODataAttribute")
                .Where(x => x.NamedArguments.Any(b => b.Key == "For" && For.Equals((string)b.Value.Value)))
                .Any() == false;
        }
    }
}
