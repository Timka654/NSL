using Microsoft.CodeAnalysis;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using NSL.Generators.Utils;
using System.Reflection.Metadata;
using System.Data.Common;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class EnumTypeGenerator
    {

        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            var type = parameter.GetTypeSymbol();

            if (type.OriginalDefinition is INamedTypeSymbol nt)
            {
                var t = nt.EnumUnderlyingType;

                if (t != null)
                {
                   var line = BaseTypeGenerator.GetReadLine(t, context, path, ignoreMembers);

                    if (line == default)
                        return "default";

                    return $"({nt.ToString()}){line}";
                }
            }

            return default;
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            var type = item.GetTypeSymbol();

            if (type.OriginalDefinition is INamedTypeSymbol nt)
            {
                var t = nt.EnumUnderlyingType;

                if (t != null)
                {
                    var line = BaseTypeGenerator.GetWriteLine(t, context, $"({t.Name}){path}", ignoreMembers);

                    return line;
                }
            }

            return default;
        }
    }
}
