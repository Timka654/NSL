using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators.CustomGenerators
{
    internal class DictionaryTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, string path)
        {
            var farg = type.TypeArguments.First();
            var sarg = type.TypeArguments.Last();


            var cb = new CodeBuilder();

            cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");
            cb.NextTab();

            cb.AppendLine($"var key = {ReadMethodsGenerator.GetValueReadSegment(farg, "key")};");

            cb.AppendLine();

            cb.AppendLine($"var value = {ReadMethodsGenerator.GetValueReadSegment(sarg, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return new {{key, value}};");

            cb.PrevTab();

            cb.AppendLine($"}}).ToDictionary(x=>x.key, x=>x.value);");

            return cb.ToString();
        }


        public static string GetWriteLine(INamedTypeSymbol type, string path)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var farg = type.TypeArguments.First();
            var sarg = type.TypeArguments.Last();

            var cb = new CodeBuilder();

            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(farg, "i.Key", null));
            cb.AppendLine();
            cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(sarg, "i.Value", null));

            cb.PrevTab();

            cb.AppendLine($"}});");

            return cb.ToString();
        }
    }
}
