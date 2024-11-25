using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class NullableTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            CodeBuilder rb = new CodeBuilder();

            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            //GenDebug.Break();

            var typedArgs = ((INamedTypeSymbol)type).TypeArguments;

            var genericType = typedArgs.FirstOrDefault();

            if (genericType == null || !genericType.IsValueType)
                return default;

            return $"dataPacket.{nameof(InputPacketBuffer.ReadNullable)}(()=>{{ return {BinaryReadMethodsGenerator.GetValueReadSegment(genericType, context, path)}; }})";
        }

        public static string GetWriteLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            var typedArgs = ((INamedTypeSymbol)type).TypeArguments;

            var genericType = typedArgs.FirstOrDefault();

            if (genericType == null || !genericType.IsValueType)
                return default;

            return $"__packet.{nameof(OutputPacketBuffer.WriteNullable)}({path},()=>{{ {BinaryWriteMethodsGenerator.BuildParameterWriter(genericType, context, $"{path}.Value")} }});";
        }
    }
}
