using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers
{
    internal class ClassTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, MethodContextModel methodContext, string path)
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

            foreach (var member in members)
            {
                ReadMethodsGenerator.AddTypeMemberReadLine(member, rb, string.Join(".", path, member.Name), methodContext);
            }

            rb.AppendLine("return data;");

            rb.PrevTab();

            rb.AppendLine("})");

            return rb.ToString();// test only
        }

        public static string GetWriteLine(ISymbol item, MethodContextModel mcm, string path)
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
                WriteMethodsGenerator.AddTypeMemberWriteLine(member, mcm, cb, string.Join(".", path, member.Name));
            }

            cb.PrevTab();

            cb.AppendLine("});");


            return cb.ToString();
        }
    }
}
