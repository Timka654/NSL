﻿using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class ClassTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (type.IsValueType)
                return default;

            CodeBuilder rb = new CodeBuilder();

            var exists = context.GetExistsReadHandleCode(parameter, path, rb);

            if (exists != default)
            {
                rb.AppendLine(exists);
                return rb.ToString();
            }


            path = $"data{++context.ProcessingLevel}";

            rb.AppendLine($"{context.IOPath}.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {{");

            rb.NextTab();

            rb.AppendLine();

            bool successOpen = context.OpenTypeEntry(parameter, path, rb, true);

            if (successOpen)
            {
                rb.AppendLine($"var {path} = new {type.GetTypeFullName(false)}();");

                rb.AppendLine();

                do
                {
                    var members = type.GetMembers()
                        .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                        .OrderBy(x => x.MetadataName);


                    //if ( ignoreMembers.Any())
                    //    GenDebug.Break();

                    foreach (var member in members)
                    {
                        var fpath = string.Join(".", path, member.Name);

                        BinaryReadMethodsGenerator.AddTypeMemberReadLine(member, context, rb, fpath);
                    }

                    type = type.BaseType;

                } while (type != null);

                rb.AppendLine($"return {path};");

                context.CloseTypeEntry(parameter, path);
            }


            rb.PrevTab();

            rb.AppendLine("})");

            --context.ProcessingLevel;

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path)
        {
            var type = item.GetTypeSymbol();

            if (type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();

            var exists = context.GetExistsWriteHandleCode(item, path, cb);

            if (exists != default)
            {
                cb.AppendLine(exists);
                return cb.ToString();
            }

            cb.AppendLine($"{context.IOPath}.{nameof(OutputPacketBuffer.WriteNullableClass)}({path}, ()=> {{");

            cb.NextTab();

            cb.AppendLine();

            bool successOpen = context.OpenTypeEntry(item, path, cb, false);

            if (successOpen)
            {
                do
                {
                    var members = type.GetMembers()
                        .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                        .OrderBy(x => x.MetadataName);

                    foreach (var member in members)
                    {
                        var fpath = string.Join(".", path, member.Name);

                        BinaryWriteMethodsGenerator.AddTypeMemberWriteLine(member, context, cb, fpath);
                    }

                    type = type.BaseType;

                } while (type != null);

                context.CloseTypeEntry(item, path);
            }

            cb.PrevTab();

            cb.AppendLine("});");

            return cb.ToString();
        }
    }
}
