using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    [BinarySerializer.BinaryPreCompile("default", 500)]
    public class StringStruct : ITestStruct<StringStruct>
    {
        private static UTF8Encoding encoding = new UTF8Encoding();

        [Binary(typeof(BinaryString16))]
        [BinaryScheme("default")]
        public string nulls16 { get; set; }

        [Binary(typeof(BinaryString32))]
        [BinaryScheme("default")]
        public string nulls32 { get; set; }

        [Binary(typeof(BinaryString16))]
        [BinaryScheme("default")]
        public string emptys16 { get; set; }

        [Binary(typeof(BinaryString32))]
        [BinaryScheme("default")]
        public string emptys32 { get; set; }

        [Binary(typeof(BinaryString16))]
        [BinaryScheme("default")]
        public string s16 { get; set; }

        [Binary(typeof(BinaryString32))]
        [BinaryScheme("default")]
        public string s32 { get; set; }

        public static StringStruct GetRndValue()
        {
            return new StringStruct().GetRandomValue();
        }

        public override StringStruct GetRandomValue()
        {
            StringStruct r = new StringStruct();

            r.emptys16 = "";
            r.emptys32 = "";


            r.s16 = Utils.GetRandomS();
            r.s32 = Utils.GetRandomS();

            return r;
        }

        public override void streamWriteFunc(Stopwatch sw)
        {
            binaryWritedValue = base.serializedValue;

            sw.Start();

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            WriteString16(bw, base.serializedValue.nulls16);
            WriteString32(bw, base.serializedValue.nulls32);
            WriteString16(bw, base.serializedValue.emptys16);
            WriteString32(bw, base.serializedValue.emptys32);
            WriteString16(bw, base.serializedValue.s16);
            WriteString32(bw, base.serializedValue.s32);

            base.streamWriteBuffer = ms.ToArray();

            sw.Stop();

        }

        public static void WriteString32(BinaryWriter bw, string v)
        {
            if (v != null)
            {
                bw.Write(false);
                var arr = encoding.GetBytes(v);
                bw.Write(arr.Length);
                bw.Write(arr);
            }
            else
            {
                bw.Write(true);
            }
        }

        public static string ReadString32(BinaryReader br)
        {
            bool isNull = br.ReadBoolean();

            if (isNull)
                return null;

            int len = br.ReadInt32();

            return encoding.GetString(br.ReadBytes(len), 0, len);
        }

        public static void WriteString16(BinaryWriter bw, string v)
        {
            if (v != null)
            {
                bw.Write(false);
                var arr = encoding.GetBytes(v);
                bw.Write((short)arr.Length);
                bw.Write(arr);
            }
            else
            {
                bw.Write(true);
            }
        }

        public static string ReadString16(BinaryReader br)
        {
            bool isNull = br.ReadBoolean();

            if (isNull)
                return null;

            short len = br.ReadInt16();

            return encoding.GetString(br.ReadBytes(len), 0, len);
        }

        public override void streamReadFunc(Stopwatch sw)
        {
            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryReader br = new BinaryReader(ms);

            StringStruct r = new StringStruct();

            sw.Start();

            r.nulls16 = ReadString16(br);
            r.nulls32 = ReadString32(br);

            r.emptys16 = ReadString16(br);
            r.emptys32 = ReadString32(br);

            r.s16 = ReadString16(br);
            r.s32 = ReadString32(br);

            sw.Stop();
            base.binaryReadedValue = r;
        }
    }
}
