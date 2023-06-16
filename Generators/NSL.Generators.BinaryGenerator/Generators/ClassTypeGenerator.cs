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
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            var type = parameter.GetTypeSymbol();

            if (type.IsValueType)
                return default;

            CodeBuilder rb = new CodeBuilder();

            rb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {{");

            rb.NextTab();

            rb.AppendLine();

            rb.AppendLine($"var data = new {type.Name}();");

            rb.AppendLine();

            path = "data";

            do
            {

                var members = type.GetMembers().OrderBy(x=>x.MetadataName);


                //if (!Debugger.IsAttached && ignoreMembers.Any())
                //    Debugger.Launch();

                foreach (var member in members)
                {
                    if (ignoreMembers != null && ignoreMembers.Any(x => x.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    BinaryReadMethodsGenerator.AddTypeMemberReadLine(member, context, rb, string.Join(".", path, member.Name));
                }

                type = type.BaseType;

            } while (type != null);

            rb.AppendLine("return data;");

            rb.PrevTab();

            rb.AppendLine("})");

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
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

                var members = type.GetMembers().OrderBy(x=>x.MetadataName);


                foreach (var member in members)
                {
                    if (ignoreMembers != null && ignoreMembers.Any(x => x.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    BinaryWriteMethodsGenerator.AddTypeMemberWriteLine(member, context, cb, string.Join(".", path, member.Name));
                }

                type = type.BaseType;

            } while (type != null);

            cb.PrevTab();

            cb.AppendLine("});");


            return cb.ToString();
        }
    }
}
