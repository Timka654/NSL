using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class StructTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            var rb = new CodeBuilder();

            if (type.IsTupleType)
            {
                //if (!Debugger.IsAttached)
                //    Debugger.Launch();

                var nts = type as INamedTypeSymbol;

                rb.AppendLine($@"new {type.Name}<{string.Join(", ", nts.TupleElements.Select(x => BinaryReadMethodsGenerator.BuildNullableTypeDef(x)))}>();");
            }
            else
                rb.AppendLine($"new {type.Name}();");

            rb.AppendLine();

            path = parameter.GetName(path) ?? default;

            var members = type.GetMembers();


            foreach (var member in members)
            {
                if (ignoreMembers != null && ignoreMembers.Any(x => x.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                BinaryReadMethodsGenerator.AddTypeMemberReadLine(member, context, rb, string.Join(".", path, member.Name));
            }

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            var type = item.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();

            var members = type.GetMembers();

            foreach (var member in members)
            {
                if (ignoreMembers != null && ignoreMembers.Any(x => x.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                BinaryWriteMethodsGenerator.AddTypeMemberWriteLine(member, context, cb, string.Join(".", path, member.Name));
            }

            return cb.ToString();
        }
    }
}
