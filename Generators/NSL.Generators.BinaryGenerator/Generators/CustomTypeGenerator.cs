using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using NSL.Generators.Utils;
using NSL.Generators.BinaryGenerator.Generators.CustomGenerators;
using System;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class CustomTypeGenerator
    {
        private static Dictionary<string, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)> customTypeReadHandlers
            = new Dictionary<string, (CustomTypeHandle readHandle, CustomTypeHandle writeHandle)>();

        private static bool GetHandlers(string typeName, out (CustomTypeHandle readHandle, CustomTypeHandle writeHandle) handlers)
        {
            if (customTypeReadHandlers.TryGetValue(typeName, out handlers))
                return true;

            handlers = default;

            return false;
        }

        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            string typeName = default;

            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type != null)
                typeName = type.MetadataName;
            else
            {
                var arrType = parameter as IArrayTypeSymbol;

                typeName = $"{arrType.ElementType.Name}[]";
            }

            if (typeName == default)
                return default;

            if (GetHandlers(typeName, out var handlers))
                return handlers.readHandle(type, context, path);
            else if (type != null)
                foreach (var item in type.AllInterfaces)
                {
                    typeName = item.MetadataName;
                    if (GetHandlers(typeName, out handlers))
                        return handlers.readHandle(type, context, path);
                }

            return default;
        }

        public static string GetWriteLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            string typeName = default;

            INamedTypeSymbol type = parameter.GetTypeSymbol() as INamedTypeSymbol;

            if (type != null)
                typeName = type.MetadataName;
            else
            {
                var arrType = parameter as IArrayTypeSymbol;

                typeName = $"{arrType.ElementType.Name}[]";
            }

            if (typeName == default)
                return default;

            if (GetHandlers(typeName, out var handlers))
                return handlers.writeHandle(type, context, path);
            else if (type != null)
                foreach (var item in type.AllInterfaces)
                {
                    typeName = item.MetadataName;
                    if (GetHandlers(typeName, out handlers))
                        return handlers.writeHandle(type, context, path);
                }

            return default;
        }

        static CustomTypeGenerator()
        {
            customTypeReadHandlers.Add(
                typeof(byte[]).Name,
                (ByteArrayTypeGenerator.GetReadLine, ByteArrayTypeGenerator.GetWriteLine));

            customTypeReadHandlers.Add(
                typeof(Span<byte>).Name,
                (ByteArrayTypeGenerator.GetSpanReadLine, ByteArrayTypeGenerator.GetSpanWriteLine));

            customTypeReadHandlers.Add(
                typeof(Dictionary<,>).Name,
                (DictionaryTypeGenerator.GetReadLine, DictionaryTypeGenerator.GetWriteLine));

            customTypeReadHandlers.Add(
                typeof(List<>).Name,
                (ListTypeGenerator.GetReadLine, ListTypeGenerator.GetWriteLine));

            customTypeReadHandlers.Add(
                typeof(IList<>).Name,
                (IListTypeGenerator.GetReadLine, IListTypeGenerator.GetWriteLine));
        }
    }
}
