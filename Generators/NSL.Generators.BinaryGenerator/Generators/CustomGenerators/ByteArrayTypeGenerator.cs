using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class ByteArrayTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            return $"dataPacket.{nameof(InputPacketBuffer.ReadNullableClass)}(() => dataPacket.{nameof(InputPacketBuffer.ReadByteArray)}());";
        }


        public static string GetWriteLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            return $"__packet.{nameof(OutputPacketBuffer.WriteNullableClass)}({path},() => __packet.{nameof(OutputPacketBuffer.WriteByteArray)}({path}));";
        }
    }
}
