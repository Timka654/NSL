using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Generators.Utils;
using NSL.Generators.BinaryGenerator.Generators.CustomGenerators;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class CustomTypeGenerator
    {
        private static Dictionary<INamedTypeSymbol, Type> customSymbTypeMap;

        private static Dictionary<Type, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)> customTypeReadHandlers
            = new Dictionary<Type, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)>();

        private static bool GetHandlers(INamedTypeSymbol type, out (CustomTypeHandle readHandle, CustomTypeHandle writeHandle) handlers)
        {
            //if (customSymbTypeMap == null)
            //    customSymbTypeMap = customTypeReadHandlers.ToDictionary(x => methodContext.Compilation.GetTypeByMetadataName(x.Key.FullName), x => x.Key);

            if (customSymbTypeMap.TryGetValue(type.OriginalDefinition, out var eqType))
                if (customTypeReadHandlers.TryGetValue(eqType, out handlers))
                    return true;

            handlers = default;

            return false;
        }

        public static string GetReadLine(ISymbol parameter, string path, IEnumerable<string> ignoreMembers)
        {
            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type == null)
                return default;

            if (GetHandlers(type, out var handlers))
                return handlers.readHandle(type, path);

            return default;
        }

        public static string GetWriteLine(ISymbol parameter, string path, IEnumerable<string> ignoreMembers)
        {
            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type == null)
                return default;

            if (GetHandlers(type, out var handlers))
                return handlers.writeHandle(type, path);

            return default;
        }

        static CustomTypeGenerator()
        {
            customTypeReadHandlers.Add(
                typeof(Dictionary<,>),
                (DictionaryTypeGenerator.GetReadLine, DictionaryTypeGenerator.GetWriteLine));

            customTypeReadHandlers.Add(typeof(List<>),
                (ListTypeGenerator.GetReadLine, ListTypeGenerator.GetWriteLine));
        }
    }
}
