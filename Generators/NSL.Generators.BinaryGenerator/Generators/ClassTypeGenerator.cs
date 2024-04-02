using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
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

            path = $"data{++context.ProcessingLevel}";

            CodeBuilder rb = new CodeBuilder();

            rb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {{");

            rb.NextTab();

            rb.AppendLine();

            rb.AppendLine($"var {path} = new {type.Name}();");

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

                    if (context.IsIgnore(member, fpath))
                        continue;

                    BinaryReadMethodsGenerator.AddTypeMemberReadLine(member, context, rb, fpath);
                }

                type = type.BaseType;

            } while (type != null);

            rb.AppendLine($"return {path};");

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


            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteNullableClass)}({path}, ()=> {{");

            cb.NextTab();

            cb.AppendLine();

            do
            {
                var members = type.GetMembers()
                    .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                    .OrderBy(x => x.MetadataName);

                foreach (var member in members)
                {
                    var fpath = string.Join(".", path, member.Name);

                    if (context.IsIgnore(member, fpath))
                        continue;

                    BinaryWriteMethodsGenerator.AddTypeMemberWriteLine(member, context, cb, fpath);
                }

                type = type.BaseType;

            } while (type != null);

            cb.PrevTab();

            cb.AppendLine("});");


            return cb.ToString();
        }
    }
}
