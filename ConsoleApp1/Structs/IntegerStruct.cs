using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class IntegerStruct : ITestStruct<IntegerStruct>
    {
        [Binary(typeof(BinaryBool))]
        public bool b { get; set; }

        [Binary(typeof(BinaryInt8))]
        public byte i8 { get; set; }

        [Binary(typeof(BinarySInt8))]
        public sbyte si8 { get; set; }

        [Binary(typeof(BinaryInt16))]
        public short i16 { get; set; }

        [Binary(typeof(BinaryUInt16))]
        public ushort ui16 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int i32 { get; set; }

        [Binary(typeof(BinaryUInt32))]
        public uint ui32 { get; set; }

        [Binary(typeof(BinaryInt64))]
        public long i64 { get; set; }

        [Binary(typeof(BinaryUInt64))]
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

            bw.Write(base.serializedValue.b);
            bw.Write(base.serializedValue.i8);
            bw.Write(base.serializedValue.si8);
            bw.Write(base.serializedValue.i16);
            bw.Write(base.serializedValue.ui16);
            bw.Write(base.serializedValue.i32);
            bw.Write(base.serializedValue.ui32);
            bw.Write(base.serializedValue.i64);
            bw.Write(base.serializedValue.ui64);

            bw.Close();

            base.streamWriteBuffer = ms.ToArray();
            sw.Stop();
        }

        private void Write()
        {

        }

        public override void streamReadFunc(Stopwatch sw)
        {
            sw.Start();
            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryReader br = new BinaryReader(ms);

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

            sw.Stop();
            if(binaryReadedValue == null)
            binaryReadedValue = r;
        }
    }
}
