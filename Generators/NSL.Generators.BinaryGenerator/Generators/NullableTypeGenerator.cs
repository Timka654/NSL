using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class NullableTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
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

            return $"dataPacket.{nameof(InputPacketBuffer.ReadNullable)}(()=>{{ return {BinaryReadMethodsGenerator.GetValueReadSegment(genericType, context, path)}; }})";
        }

        public static string GetWriteLine(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            var genericType = ((INamedTypeSymbol)type).TypeArguments.First();

            if (!genericType.IsValueType)
                return default;

            return $"__packet.{nameof(OutputPacketBuffer.WriteNullable)}({path},()=>{{ {BinaryWriteMethodsGenerator.BuildParameterWriter(genericType, context, $"{path}.Value", null)} }});";
        }
    }
}
