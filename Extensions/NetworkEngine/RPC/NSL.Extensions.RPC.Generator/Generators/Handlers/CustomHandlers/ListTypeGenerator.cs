using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers.CustomHandlers
{
    internal class ListTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, MethodContextModel methodContext, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");

            cb.NextTab();

            cb.AppendLine($"var value = {ReadMethodsGenerator.GetValueReadSegment(farg, methodContext, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return value;");

            cb.PrevTab();

            cb.AppendLine($"}}).ToList();");

            return cb.ToString();
        }

        public static string GetWriteLine(INamedTypeSymbol type, MethodContextModel methodContext, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(farg, methodContext, "i"));

            cb.PrevTab();

            cb.AppendLine($"}});");


            return cb.ToString();
        }
    }
}
