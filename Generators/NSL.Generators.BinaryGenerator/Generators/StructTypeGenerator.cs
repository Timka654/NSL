﻿using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class StructTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            var rb = new CodeBuilder();

            var exists = context.GetExistsReadHandleCode(parameter, path, rb);

            if (exists != default)
            {
                rb.AppendLine(exists);
                return rb.ToString();
            }

            if (type.IsTupleType)
            {
                //GenDebug.Break();

                var nts = type as INamedTypeSymbol;

                rb.AppendLine($@"new {type.Name}<{string.Join(", ", nts.TupleElements.Select(x => BinaryReadMethodsGenerator.BuildNullableTypeDef(x)))}>();");
            }
            else
                rb.AppendLine($"new {type.Name}();");

            rb.AppendLine();

            bool successOpen = context.OpenTypeEntry(parameter, path, rb, true);

            if (successOpen)
            {
                path = parameter.GetName(path) ?? default;

                var members = type.GetMembers()
                    .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                    .OrderBy(x => x.MetadataName);


                foreach (var member in members)
                {
                    var fpath = string.Join(".", path, member.Name);

                    BinaryReadMethodsGenerator.AddTypeMemberReadLine(member, context, rb, fpath);
                }

                context.CloseTypeEntry(parameter, path);
            }

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path)
        {
            var type = item.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();

            var exists = context.GetExistsWriteHandleCode(item, path, cb);

            if (exists != default)
            {
                cb.AppendLine(exists);
                return cb.ToString();
            }

            bool successOpen = context.OpenTypeEntry(item, path, cb, false);

            if (successOpen)
            {
                var members = type.GetMembers()
                    .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                    .OrderBy(x => x.MetadataName);

                foreach (var member in members)
                {
                    var fpath = string.Join(".", path, member.Name);

                    BinaryWriteMethodsGenerator.AddTypeMemberWriteLine(member, context, cb, fpath);
                }

                context.CloseTypeEntry(item, path);
            }

            return cb.ToString();
        }
    }
}
