using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.Generators.BinaryGenerator.Generators;
using NSL.Generators.BinaryGenerator.Utils;

namespace NSL.Generators.BinaryGenerator
{
    public class BinaryWriteMethodsGenerator
    {
        private static List<GenerateHandle> generators = new List<GenerateHandle>();

        static BinaryWriteMethodsGenerator()
        {
            generators.Add(CustomTypeGenerator.GetWriteLine);
            generators.Add(ArrayTypeGenerator.GetWriteLine);
            generators.Add(EnumTypeGenerator.GetWriteLine);
            generators.Add(BaseTypeGenerator.GetWriteLine);
            generators.Add(NullableTypeGenerator.GetWriteLine);
            generators.Add(ClassTypeGenerator.GetWriteLine);
            generators.Add(StructTypeGenerator.GetWriteLine);
        }

        //public static string BuildParameterWriter(ISymbol item, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        public static string BuildParameterWriter(ISymbol item, BinaryGeneratorContext context, string path)
        {
            string writerLine = default;

            foreach (var gen in generators)
            {
                writerLine = gen(item, context, path);

                if (writerLine != default)
                    return writerLine;
            }

            context.Context.ShowBIODiagnostics("NSLBIO002", $"Not found write generator for this type", DiagnosticSeverity.Error, item.Locations.ToArray());

            return writerLine;
        }


        public static bool AddTypeMemberWriteLine(ISymbol member, BinaryGeneratorContext context, CodeBuilder cb, string path)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return false;

            if (context.IsIgnore(member, path))
                return false;

            ITypeSymbol type = null;

            if (member is IPropertySymbol ps)
            {
                if (ps.GetMethod == null)
                {
                    context.Context.ShowBIODiagnostics("NSLBIO004", $"Not found getter for this property - ignore on write", DiagnosticSeverity.Warning, member.Locations.ToArray());

                    return false;
                }

                type = ps.GetTypeSymbol();
            }
            else if (member is IFieldSymbol fs)
            {
                type = fs.GetTypeSymbol();
            }
            else
                return false;

            context.CurrentMember = member;

            cb.AppendLine(BuildParameterWriter(type, context, path));

            cb.AppendLine();

            return true;
        }
    }
}
