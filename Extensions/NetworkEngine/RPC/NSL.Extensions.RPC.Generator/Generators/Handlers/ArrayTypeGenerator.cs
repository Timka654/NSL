using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Generators.Handlers
{
    internal class ArrayTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, MethodContextModel methodContext, string path)
        {
            if (parameter is IArrayTypeSymbol array)
            {
                var farg = array.ElementType;

                var cb = new CodeBuilder();

                cb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadCollection)}(()=>{{");

                cb.NextTab();

                cb.AppendLine($"var value = {ReadMethodsGenerator.GetValueReadSegment(farg, methodContext, "value")};");

                cb.AppendLine();

                cb.AppendLine($"return value;");

                cb.PrevTab();

                cb.AppendLine($"}}).ToArray();");

                return cb.ToString();
            }

            return default;
        }


        public static string GetWriteLine(ISymbol parameter, MethodContextModel methodContext, string path)
        {
            if (parameter is IArrayTypeSymbol array)
            {
                var farg = array.ElementType;

                var cb = new CodeBuilder();

                cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteCollection)}({path},(i)=>{{");

                cb.NextTab();

                cb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(farg, methodContext, "i"));

                cb.PrevTab();

                cb.AppendLine($"}});");


                return cb.ToString();
            }

            return default;
        }
    }
}
