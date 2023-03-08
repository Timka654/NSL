using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers
{
    internal class NullableTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, MethodContextModel methodContext, string path, IEnumerable<string> ignoreMembers)
        {
            CodeBuilder rb = new CodeBuilder();

            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var genericType = ((INamedTypeSymbol)type).TypeArguments.First();

            if (!genericType.IsValueType)
                return default;

            return $"dataPacket.{nameof(InputPacketBuffer.ReadNullable)}(()=>{{ return {ReadMethodsGenerator.GetValueReadSegment(genericType, methodContext, path)}; }})";
        }

        public static string GetWriteLine(ISymbol parameter, MethodContextModel mcm, string path, IEnumerable<string> ignoreMembers)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            var genericType = ((INamedTypeSymbol)type).TypeArguments.First();

            if (!genericType.IsValueType)
                return default;

            return $"__packet.{nameof(OutputPacketBuffer.WriteNullable)}({path},()=>{{ {WriteMethodsGenerator.BuildParameterWriter(genericType, mcm, $"{path}.Value", null)} }});";
        }
    }
}
