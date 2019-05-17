using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class IntegerStruct
    {
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

        public static IntegerStruct GetRandomValue()
        {
            IntegerStruct r = new IntegerStruct();

            r.i8 = Utils.GetRandomI8();
            r.si8 = Utils.GetRandomSI8();

            r.i16 = Utils.GetRandomI16();
            r.ui16 = Utils.GetRandomUI16();

            r.i32 = Utils.GetRandomI32();
            r.ui32 = Utils.GetRandomUI32();

            r.i64 = Utils.GetRandomI64();
            r.ui64 = Utils.GetRandomUI64();

            return r;
        }

        private static
            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        public static byte[] buffer;

        public static IntegerStruct desValue;

        public static Action<Stopwatch> bsSerializeAction = new Action<Stopwatch>((sw) =>
        {
            var r = GetRandomValue();

            if (buffer == null)
                buffer = bs.Serialize(r, "");

            sw.Start();
            bs.Serialize(r, "");
            sw.Stop();
        });

        public static Action<Stopwatch> bsDesserilize = new Action<Stopwatch>((sw) =>
        {
            if (desValue == null)
                desValue = bs.Desserialize<IntegerStruct>(buffer, "");


            sw.Start();
            bs.Desserialize<IntegerStruct>(buffer, "");
            sw.Stop();
        });
    }
}
