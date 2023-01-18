using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers.CustomHandlers
{
    internal class DictionaryTypeGenerator
    {
        public static string GetReadLine(INamedTypeSymbol type, MethodContextModel methodContext, string path)
        {
            var farg = type.TypeArguments.First();
            var sarg = type.TypeArguments.Last();


            var cb = new CodeBuilder();

            cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");
            cb.NextTab();

            cb.AppendLine($"var key = {ReadMethodsGenerator.GetValueReadSegment(farg, methodContext, "key")};");

            cb.AppendLine();

            cb.AppendLine($"var value = {ReadMethodsGenerator.GetValueReadSegment(sarg, methodContext, "value")};");

            cb.AppendLine();

            cb.AppendLine($"return new {{key, value}};");

            cb.PrevTab();

            cb.AppendLine($"}}).ToDictionary(x=>x.key, x=>x.value);");

            return cb.ToString();
        }


        public static string GetWriteLine(INamedTypeSymbol type, MethodContextModel methodContext, string path)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var farg = type.TypeArguments.First();
            var sarg = type.TypeArguments.Last();

            var cb = new CodeBuilder();

            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

            cb.NextTab();

            cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(farg, methodContext, "i.Key", null));
            cb.AppendLine();
            cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(sarg, methodContext, "i.Value", null));

            cb.PrevTab();

            cb.AppendLine($"}});");

            return cb.ToString();
        }
    }
}
