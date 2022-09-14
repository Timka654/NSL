using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Utils
{
    internal static class SymbolExtensions
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
    }
}
