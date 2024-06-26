﻿using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
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

            if (type.IsTupleType)
            {
                //GenDebug.Break();

                var nts = type as INamedTypeSymbol;

                rb.AppendLine($@"new {type.Name}<{string.Join(", ", nts.TupleElements.Select(x => BinaryReadMethodsGenerator.BuildNullableTypeDef(x)))}>();");
            }
            else
                rb.AppendLine($"new {type.Name}();");

            rb.AppendLine();

            path = parameter.GetName(path) ?? default;

            var members = type.GetMembers()
                .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                .OrderBy(x => x.MetadataName);


            foreach (var member in members)
            {
                var fpath = string.Join(".", path, member.Name);

                if (context.IsIgnore(member, fpath))
                    continue;

                BinaryReadMethodsGenerator.AddTypeMemberReadLine(member, context, rb, fpath);
            }

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path)
        {
            var type = item.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();

            var members = type.GetMembers()
                .Where(x=>x is IFieldSymbol || x is IPropertySymbol)
                .OrderBy(x=>x.MetadataName);

            foreach (var member in members)
            {
                var fpath = string.Join(".", path, member.Name);

                if (context.IsIgnore(member, fpath))
                    continue;

                BinaryWriteMethodsGenerator.AddTypeMemberWriteLine(member, context, cb, fpath);
            }

            return cb.ToString();
        }
    }
}
