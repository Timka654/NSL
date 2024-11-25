using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class EnumTypeGenerator
    {

        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (type.OriginalDefinition is INamedTypeSymbol nt)
            {
                var t = nt.EnumUnderlyingType;

                if (t != null)
                {
                   var line = BaseTypeGenerator.GetReadLine(t, context, path);

                    if (line == default)
                        return "default";

                    return $"({nt.ToString()}){line}";
                }
            }

            return default;
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path)
        {
            var type = item.GetTypeSymbol();

            if (type.OriginalDefinition is INamedTypeSymbol nt)
            {
                var t = nt.EnumUnderlyingType;

                if (t != null)
                {
                    var line = BaseTypeGenerator.GetWriteLine(t, context, $"({t.Name}){path}");

                    return line;
                }
            }

            return default;
        }
    }
}
