using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class IListTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            //var b = new InputPacketBuffer();
            //b.ReadNullableClass(()=> b.ReadCollection<int>(()=>b.ReadInt32()))

            var cParamType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(IEnumerable<>).FullName).Construct(farg);

            var constructor = type.Constructors.FirstOrDefault(x => x.Parameters.FirstOrDefault()?.Type.Equals(cParamType) == true);

            if (constructor == null)
            {
                cParamType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(IEnumerable<>).FullName);

                constructor = type.Constructors.FirstOrDefault(x => x.Parameters.FirstOrDefault()?.Type.Equals(cParamType) == true);
            }

            if (constructor == default)
                return default;

            cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadNullableClass)}(() => new {type.ToDisplayString()}(dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");

            cb.NextTab();

            cb.AppendLine($"var value = {BinaryReadMethodsGenerator.GetValueReadSegment(farg, context, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return value;");

            cb.PrevTab();

            cb.AppendLine($"}})?.ToList()));");

            return cb.ToString();
        }

        public static string GetWriteLine(INamedTypeSymbol type, BinaryGeneratorContext context, string path)
        {
            var farg = type.TypeArguments.First();

            var cb = new CodeBuilder();

            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteNullableClass)}({path},() => __packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(BinaryWriteMethodsGenerator.BuildParameterWriter(farg, context, "i"));

            cb.PrevTab();

            cb.AppendLine($"}}));");


            return cb.ToString();
        }
    }
}
