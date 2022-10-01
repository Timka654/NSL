using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers
{
    internal class StructTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, MethodContextModel methodContext, string path)
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

                rb.AppendLine($@"new {type.Name}<{string.Join(", ", nts.TupleElements.Select(x => ReadMethodsGenerator. BuildNullableTypeDef(x)))}>();");
            }
            else
                rb.AppendLine($"new {type.Name}();");

            rb.AppendLine();

            path = parameter.GetName(path) ?? default;

            var members = type.GetMembers();


            foreach (var member in members)
            {
                ReadMethodsGenerator.AddTypeMemberReadLine(member, rb, string.Join(".", path, member.Name), methodContext);
            }

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, MethodContextModel mcm, string path)
        {
            var type = item.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();

            var members = type.GetMembers();

            foreach (var member in members)
            {
                WriteMethodsGenerator.AddTypeMemberWriteLine(member, mcm, cb, string.Join(".", path, member.Name));
            }

            return cb.ToString();
        }
    }
}
