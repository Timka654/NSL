using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class BinaryTypeIOGeneratorContext : BinaryGeneratorContext
    {
        public string ProcessingType { get; set; }

        public string ReadCurrentTypeMethodName { get; set; } = default;
        public string WriteCurrentTypeMethodName { get; set; } = default;

        public bool IsStatic { get; set; } = false;

        public Func<string, bool> ModelSelector { get; set; }

        public BinaryTypeIOGeneratorContext()
        {
            For = "*";
            ModelSelector = mname => string.Equals(For, mname);
        }

        public override bool IsIgnore(ISymbol symbol, string path)
        {
            if (HasIgnore(symbol))
                return true;

            if (For.Equals("*"))
                return false;

            //var attr = symbol.GetAttributes()
            //    .Where(x => x.AttributeClass.Name == nameof(BinaryIODataAttribute)).FirstOrDefault();

            return symbol.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(BinaryIODataAttribute))
                .Where(x => x.ConstructorArguments.Any(b => b.Values.Any(n => For.Equals(n.Value))))
                .Any() == false;
        }

        private bool HasIgnore(ISymbol symbol)
            => symbol.GetAttributes()
                    .Where(x => x.AttributeClass.Name == nameof(BinaryIODataIgnoreAttribute))
                    .Where(x => x.ConstructorArguments.Any(b=>b.Values.Length == 0) || x.ConstructorArguments.Any(b => b.Values.Any(n => For.Equals(n.Value))))
                    .Any();

        public override string GetExistsReadHandleCode(ISymbol symbol, string path, CodeBuilder codeBuilder)
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
                return $"{ReadCurrentTypeMethodName}({IOPath});";

            return base.GetExistsReadHandleCode(symbol, path, codeBuilder);
        }

        public override string GetExistsWriteHandleCode(ISymbol symbol, string path, CodeBuilder codeBuilder)
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

            if (isThis)
                return $"{WriteCurrentTypeMethodName}({path},{IOPath});";

            return base.GetExistsReadHandleCode(symbol, path, codeBuilder);
        }
    }
}
