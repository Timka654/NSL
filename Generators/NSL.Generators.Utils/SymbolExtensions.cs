using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NSL.Generators.Utils
{
    public static class SymbolExtensions
    {
        public static string GetName(this ISymbol symbol, string path)
        {
            if (symbol is IParameterSymbol param)
                return param.Name;
            if (symbol is IFieldSymbol fi)
                return fi.Name;
            if (symbol is IPropertySymbol prop)
                return prop.Name;

            return path;
        }

        public static ITypeSymbol GetTypeSymbol(this ISymbol symbol)
        {
            if (symbol is IParameterSymbol param)
                return param.Type;

            if (symbol is IPropertySymbol pr)
                return pr.Type;

            if (symbol is IFieldSymbol fi)
                return fi.Type;

            if (symbol is INamedTypeSymbol typedSymbol)
                return typedSymbol;

            return default;
        }

        public static object GetNamedArgumentValue(this AttributeData attr, string name)
        {
            var args = attr.NamedArguments;

            if (args.IsDefaultOrEmpty || !args.Any(x=>x.Key.Equals(name)))
                return default;

            return args.First(x => x.Key.Equals(name)).Value;
        }
    }
}
