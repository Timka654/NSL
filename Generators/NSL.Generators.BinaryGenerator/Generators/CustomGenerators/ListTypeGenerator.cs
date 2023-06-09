using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class ListTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");

            cb.NextTab();

            cb.AppendLine($"var value = {BinaryReadMethodsGenerator.GetValueReadSegment(farg, context, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return value;");

            cb.PrevTab();

            cb.AppendLine($"}}).ToList();");

            return cb.ToString();
        }

        public static string GetWriteLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(BinaryWriteMethodsGenerator.BuildParameterWriter(farg, context, "i", null));

            cb.PrevTab();

            cb.AppendLine($"}});");


            return cb.ToString();
        }
    }
}
