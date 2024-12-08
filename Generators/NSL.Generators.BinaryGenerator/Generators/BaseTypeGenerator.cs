using Microsoft.CodeAnalysis;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using NSL.Generators.Utils;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class BaseTypeGenerator
    {

        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!readTypeHandlers.TryGetValue(type.Name, out var tReadLine))
                return default;

            return $"{context.IOPath}.{tReadLine}()";
        }

        public static string GetWriteLine(ISymbol item, BinaryGeneratorContext context, string path)
        {
            var type = item.GetTypeSymbol();

            if (writeTypeHandlers.TryGetValue(type.Name, out var baseValueWriter))
                return $"{context.IOPath}.{baseValueWriter}({path});";

            return default;
        }

        private static Dictionary<string, string> readTypeHandlers = new Dictionary<string, string>()
        {
            {
                typeof(bool).Name,
                nameof(InputPacketBuffer.ReadBool)
            },
            {
                typeof(byte).Name,
                nameof(InputPacketBuffer.ReadByte)
            },
            {
                typeof(short).Name,
                nameof(InputPacketBuffer.ReadInt16)
            },
            {
                typeof(int).Name,
                nameof(InputPacketBuffer.ReadInt32)
            },
            {
                typeof(long).Name,
                nameof(InputPacketBuffer.ReadInt64)
            },
            {
                typeof(ushort).Name,
                nameof(InputPacketBuffer.ReadUInt16)
            },
            {
                typeof(uint).Name,
                nameof(InputPacketBuffer.ReadUInt32)
            },
            {
                typeof(ulong).Name,
                nameof(InputPacketBuffer.ReadUInt64)
            },
            {
                typeof(float).Name,
                nameof(InputPacketBuffer.ReadFloat)
            },
            {
                typeof(double).Name,
                nameof(InputPacketBuffer.ReadDouble)
            },
            {
                typeof(string).Name,
                nameof(InputPacketBuffer.ReadString)
            },
            {
                typeof(DateTime).Name,
                nameof(InputPacketBuffer.ReadDateTime)
            },
            {
                typeof(TimeSpan).Name,
                nameof(InputPacketBuffer.ReadTimeSpan)
            },
            {
                typeof(Guid).Name,
                nameof(InputPacketBuffer.ReadGuid)
            },
        };

        private static Dictionary<string, string> writeTypeHandlers = new Dictionary<string, string>()
        {
            {
                typeof(bool).Name,
                nameof(OutputPacketBuffer.WriteBool)
            },
            {
                typeof(byte).Name,
                nameof(OutputPacketBuffer.WriteByte)
            },
            {
                typeof(short).Name,
                nameof(OutputPacketBuffer.WriteInt16)
            },
            {
                typeof(int).Name,
                nameof(OutputPacketBuffer.WriteInt32)
            },
            {
                typeof(long).Name,
                nameof(OutputPacketBuffer.WriteInt64)
            },
            {
                typeof(ushort).Name,
                nameof(OutputPacketBuffer.WriteUInt16)
            },
            {
                typeof(uint).Name,
                nameof(OutputPacketBuffer.WriteUInt32)
            },
            {
                typeof(ulong).Name,
                nameof(OutputPacketBuffer.WriteUInt64)
            },
            {
                typeof(float).Name,
                nameof(OutputPacketBuffer.WriteFloat)
            },
            {
                typeof(double).Name,
                nameof(OutputPacketBuffer.WriteDouble)
            },
            {
                typeof(string).Name,
                nameof(OutputPacketBuffer.WriteString)
            },
            {
                typeof(DateTime).Name,
                nameof(OutputPacketBuffer.WriteDateTime)
            },
            {
                typeof(TimeSpan).Name,
                nameof(OutputPacketBuffer.WriteTimeSpan)
            },
            {
                typeof(Guid).Name,
                nameof(OutputPacketBuffer.WriteGuid)
            },
        };
    }
}
