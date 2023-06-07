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
        public static string GetReadLine(ISymbol parameter, string path, IEnumerable<string> ignoreMembers)
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

            var members = type.GetMembers();


            path = "data";

            //if (!Debugger.IsAttached && ignoreMembers.Any())
            //    Debugger.Launch();

            foreach (var member in members)
            {
                if (ignoreMembers != null && ignoreMembers.Any(x => x.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                ReadMethodsGenerator.AddTypeMemberReadLine(member, rb, string.Join(".", path, member.Name));
            }

            rb.AppendLine("return data;");

            rb.PrevTab();

            rb.AppendLine("})");

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, string path, IEnumerable<string> ignoreMembers)
        {
            var type = item.GetTypeSymbol();

            if (type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();


            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteNullableClass)}({path}, ()=> {{");

            cb.NextTab();

            cb.AppendLine();

            var members = type.GetMembers();


            foreach (var member in members)
            {
                if (ignoreMembers != null && ignoreMembers.Any(x => x.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                WriteMethodsGenerator.AddTypeMemberWriteLine(member, cb, string.Join(".", path, member.Name));
            }

            cb.PrevTab();

            cb.AppendLine("});");


            return cb.ToString();
        }
    }
}
