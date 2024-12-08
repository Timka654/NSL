using Microsoft.CodeAnalysis;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class ByteArrayTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            return $"{context.IOPath}.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {context.IOPath}.{nameof(InputPacketBuffer.ReadByteArray)}().ToArray());";
        }

 
        public static string GetWriteLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            return $"{context.IOPath}.{nameof(OutputPacketBuffer.WriteNullableClass)}({path},() => {context.IOPath}.{nameof(OutputPacketBuffer.WriteByteArray)}({path}));";
        }
        public static string GetSpanReadLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            return $"{context.IOPath}.{nameof(InputPacketBuffer.ReadByteArray)}();";
        }


        public static string GetSpanWriteLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            return $"{context.IOPath}.{nameof(OutputPacketBuffer.WriteByteArray)}({path});";
        }
    }
}
