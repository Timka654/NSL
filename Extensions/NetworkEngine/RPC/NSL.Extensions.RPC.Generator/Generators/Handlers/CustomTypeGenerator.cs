using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Generators.Handlers.CustomHandlers;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers
{
    internal class CustomTypeGenerator
    {

        private static Dictionary<INamedTypeSymbol, Type> customSymbTypeMap;

        private static Dictionary<Type, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)> customTypeReadHandlers
            = new Dictionary<Type, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)>();

        private static bool GetHandlers(INamedTypeSymbol type, MethodContextModel methodContext, out (CustomTypeHandle readHandle, CustomTypeHandle writeHandle) handlers)
        {
            if (customSymbTypeMap == null)
                customSymbTypeMap = customTypeReadHandlers.ToDictionary(x => methodContext.Compilation.GetTypeByMetadataName(x.Key.FullName), x => x.Key);

            if (customSymbTypeMap.TryGetValue(type.OriginalDefinition, out var eqType))
                if (customTypeReadHandlers.TryGetValue(eqType, out handlers))
                    return true;

            handlers = default;

            return false;
        }

        public static string GetReadLine(ISymbol parameter, MethodContextModel methodContext, string path)
        {
            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type == null)
                return default;

            if (GetHandlers(type, methodContext, out var handlers))
                    return handlers.readHandle(type, methodContext, path);

            return default;
        }

        public static string GetWriteLine(ISymbol parameter, MethodContextModel methodContext, string path)
        {
            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type == null)
                return default;

            if (GetHandlers(type, methodContext, out var handlers))
                return handlers.writeHandle(type, methodContext, path);

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
