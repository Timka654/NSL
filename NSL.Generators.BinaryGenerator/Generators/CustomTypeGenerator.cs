using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Generators.Utils;
using NSL.Generators.BinaryGenerator.Generators.CustomGenerators;
using System.Diagnostics;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class CustomTypeGenerator
    {
        private static Dictionary<string, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)> customTypeReadHandlers
            = new Dictionary<string, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)>();

        private static bool GetHandlers(INamedTypeSymbol type, out (CustomTypeHandle readHandle, CustomTypeHandle writeHandle) handlers)
        {
            if (customTypeReadHandlers.TryGetValue(type.MetadataName, out handlers))
                return true;

            handlers = default;

            return false;
        }

        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type == null)
                return default;

            if (GetHandlers(type, out var handlers))
                return handlers.readHandle(type, context, path);

            return default;
        }

        public static string GetWriteLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type == null)
                return default;

            if (GetHandlers(type, out var handlers))
                return handlers.writeHandle(type, context, path);

            return default;
        }

        static CustomTypeGenerator()
        {
            customTypeReadHandlers.Add(
                typeof(Dictionary<,>).Name,
                (DictionaryTypeGenerator.GetReadLine, DictionaryTypeGenerator.GetWriteLine));

            customTypeReadHandlers.Add(
                typeof(List<>).Name,
                (ListTypeGenerator.GetReadLine, ListTypeGenerator.GetWriteLine));
        }
    }
}
