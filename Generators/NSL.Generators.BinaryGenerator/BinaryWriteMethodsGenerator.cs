using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.Generators.BinaryGenerator.Generators;

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

        public static string BuildParameterWriter(ISymbol item, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            string writerLine = default;

            if (ignoreMembers == null || !ignoreMembers.Any(x => x.Equals("*")))
            {
                foreach (var gen in generators)
                {
                    writerLine = gen(item, context, path, ignoreMembers);

                    if (writerLine != default)
                        break;
                }
            }

            return writerLine ?? ""; //debug only
        }


        public static void AddTypeMemberWriteLine(ISymbol member, BinaryGeneratorContext context, CodeBuilder cb, string path)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (context.IsIgnore(member))
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = ps.GetTypeSymbol();
                    cb.AppendLine(BuildParameterWriter(ptype, context, path, null));

                    cb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = fs.GetTypeSymbol();
                cb.AppendLine(BuildParameterWriter(ftype, context, path, null));
                cb.AppendLine();
            }
        }
    }
}
