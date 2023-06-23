using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class BinaryTypeIOGeneratorContext : BinaryGeneratorContext
    {
        public string For { get; set; } = "*";

        public string ProcessingType { get; set; }

        public string ReadCurrentTypeMethodName { get; set; } = default;
        public string WriteCurrentTypeMethodName { get; set; } = default;

        public bool IsStatic { get; set; } = false;

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

        public override string GetExistsReadHandleCode(ISymbol symbol, string path)
        {
            bool isThis = false;

            if (path != null)
            {
                if (symbol is INamedTypeSymbol nts)
                {
                    if (nts.MetadataName.Equals(ProcessingType)) 
                        isThis = true;
                }
                if (symbol is IArrayTypeSymbol ats)
                {
                    if (ats.ToString().Equals(ProcessingType))
                        isThis = true;
                }
            }

            if (isThis)
                return $"{ReadCurrentTypeMethodName}(dataPacket);";

            return base.GetExistsReadHandleCode(symbol, path);
        }

        public override string GetExistsWriteHandleCode(ISymbol symbol, string path)
        {
            bool isThis = false;

            if (!path.Equals("value") && !path.Equals("this"))
            {
                if (symbol is INamedTypeSymbol nts)
                {
                    if (nts.MetadataName.Equals(ProcessingType))
                        isThis = true;
                }
                if (symbol is IArrayTypeSymbol ats)
                {
                    if (ats.ToString().Equals(ProcessingType))
                        isThis = true;
                }
            }
            
            if(isThis)
                return $"{WriteCurrentTypeMethodName}({path},__packet);";

            return base.GetExistsReadHandleCode(symbol, path);
        }
    }
}
