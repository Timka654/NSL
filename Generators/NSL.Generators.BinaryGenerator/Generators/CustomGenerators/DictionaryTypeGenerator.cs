using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class DictionaryTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            var farg = type.TypeArguments.First();
            var sarg = type.TypeArguments.Last();


            var cb = new CodeBuilder();

            cb.AppendLine($"{context.IOPath}.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {context.IOPath}.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");
            cb.NextTab();

            cb.AppendLine($"var key = {BinaryReadMethodsGenerator.GetValueReadSegment(farg, context, "key")};");

            cb.AppendLine();

            cb.AppendLine($"var value = {BinaryReadMethodsGenerator.GetValueReadSegment(sarg, context, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return new {{key, value}};");

            cb.PrevTab();

            cb.AppendLine($"}})?.ToDictionary(x=>x.key, x=>x.value));");

            return cb.ToString();
        }


        public static string GetWriteLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            //GenDebug.Break();

            var farg = type.TypeArguments.First();
            var sarg = type.TypeArguments.Last();

            var cb = new CodeBuilder();

            cb.AppendLine($"{context.IOPath}.{nameof(OutputPacketBuffer.WriteNullableClass)}({path},() => {context.IOPath}.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(BinaryWriteMethodsGenerator.BuildParameterWriter(farg, context, "i.Key"));
            cb.AppendLine();
            cb.AppendLine(BinaryWriteMethodsGenerator.BuildParameterWriter(sarg, context, "i.Value"));

            cb.PrevTab();

            cb.AppendLine($"}}));");

            return cb.ToString();
        }
    }
}
