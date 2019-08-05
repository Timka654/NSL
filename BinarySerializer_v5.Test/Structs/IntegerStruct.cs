using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    [BinarySerializer.BinaryPreCompile("default", 30)]
    public class IntegerStruct : ITestStruct<IntegerStruct>
    {
        [Binary(typeof(BinaryBool))]
        [BinaryScheme("default")]
        public bool b { get; set; }

        [Binary(typeof(BinaryInt8))]
        [BinaryScheme("default")]
        public byte i8 { get; set; }

        [Binary(typeof(BinarySInt8))]
        [BinaryScheme("default")]
        public sbyte si8 { get; set; }

        [Binary(typeof(BinaryInt16))]
        [BinaryScheme("default")]
        public short i16 { get; set; }

        [Binary(typeof(BinaryUInt16))]
        [BinaryScheme("default")]
        public ushort ui16 { get; set; }

        [Binary(typeof(BinaryInt32))]
        [BinaryScheme("default")]
        public int i32 { get; set; }

        [Binary(typeof(BinaryUInt32))]
        [BinaryScheme("default")]
        public uint ui32 { get; set; }

        [Binary(typeof(BinaryInt64))]
        [BinaryScheme("default")]
        public long i64 { get; set; }

        [Binary(typeof(BinaryUInt64))]
        [BinaryScheme("default")]
        public ulong ui64 { get; set; }

        public static IntegerStruct GetRndValue()
        {
            return new IntegerStruct().GetRandomValue();
        }

        public override IntegerStruct GetRandomValue()
        {
            IntegerStruct r = new IntegerStruct();

            r.b = Utils.GetRandomBool();

            r.i8 = Utils.GetRandomI8();
            r.si8 = Utils.GetRandomSI8();

            r.i16 = Utils.GetRandomI16();
            r.ui16 = Utils.GetRandomUI16();

            r.i32 = Utils.GetRandomI32();
            r.ui32 = Utils.GetRandomUI32();

            r.i64 = Utils.GetRandomI64();
            r.ui64 = Utils.GetRandomUI64();

            normalValue = r;

            return r;
        }

        public override void streamWriteFunc(Stopwatch sw)
        {
            binaryWritedValue = base.serializedValue;
            sw.Start();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            Write(bw, base.serializedValue);

            bw.Close();

            base.streamWriteBuffer = ms.ToArray();
            sw.Stop();
        }

        public static void Write(BinaryWriter bw, IntegerStruct ist)
        {

            bw.Write(ist.b);
            bw.Write(ist.i8);
            bw.Write(ist.si8);
            bw.Write(ist.i16);
            bw.Write(ist.ui16);
            bw.Write(ist.i32);
            bw.Write(ist.ui32);
            bw.Write(ist.i64);
            bw.Write(ist.ui64);
        }

        public static IntegerStruct Read(BinaryReader br)
        {
            IntegerStruct r = new IntegerStruct();
            r.b = br.ReadBoolean();

            r.i8 = br.ReadByte();
            r.si8 = br.ReadSByte();

            r.i16 = br.ReadInt16();
            r.ui16 = br.ReadUInt16();

            r.i32 = br.ReadInt32();
            r.ui32 = br.ReadUInt32();

            r.i64 = br.ReadInt64();
            r.ui64 = br.ReadUInt64();

            return r;
        }

        public override void streamReadFunc(Stopwatch sw)
        {
            sw.Start();
            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryReader br = new BinaryReader(ms);

            IntegerStruct r = Read(br);

            sw.Stop();
            if(binaryReadedValue == null)
            binaryReadedValue = r;
        }
    }
}
