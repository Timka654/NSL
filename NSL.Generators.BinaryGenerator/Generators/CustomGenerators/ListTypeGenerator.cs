using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class ListTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");

            cb.NextTab();

            cb.AppendLine($"var value = {ReadMethodsGenerator.GetValueReadSegment(farg, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return value;");

            cb.PrevTab();

            cb.AppendLine($"}}).ToList();");

            return cb.ToString();
        }

        public static string GetWriteLine(INamedTypeSymbol type, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(farg, "i", null));

            cb.PrevTab();

            cb.AppendLine($"}});");


            return cb.ToString();
        }
    }
}
