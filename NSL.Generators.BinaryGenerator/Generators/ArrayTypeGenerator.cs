using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class ArrayTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, string path, IEnumerable<string> ignoreMembers)
        {
            if (parameter is IArrayTypeSymbol array)
            {
                var farg = array.ElementType;

                var cb = new CodeBuilder();

                cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");

                cb.NextTab();

                cb.AppendLine($"var value = {ReadMethodsGenerator.GetValueReadSegment(farg, "value")};");

                cb.AppendLine();

                cb.AppendLine($"return value;");

                cb.PrevTab();

                cb.AppendLine($"}}).ToArray();");

                return cb.ToString();
            }

            return default;
        }


        public static string GetWriteLine(ISymbol parameter, string path, IEnumerable<string> ignoreMembers)
        {
            if (parameter is IArrayTypeSymbol array)
            {
                var farg = array.ElementType;

                var cb = new CodeBuilder();

                cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

                cb.NextTab();

                cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(farg, "i", null));

                cb.PrevTab();

                cb.AppendLine($"}});");


                return cb.ToString();
            }

            return default;
        }
    }
}
