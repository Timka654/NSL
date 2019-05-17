using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class ArrayStruct
    {
        [Binary(typeof(BinaryArray16<BinaryFloat32>))]
        public float[] fa32 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        public int[] ia32 { get; set; }

        [Binary(typeof(BinaryArray16<IntegerStruct>))]
        public IntegerStruct[] isa { get; set; }

        [Binary(typeof(BinaryArray16<BinaryString16>))]
        public string[] sv { get; set; }

        public static ArrayStruct GetRandomValue()
        {
            ArrayStruct r = new ArrayStruct();

            int temp = 0;

            r.fa32 = new float[temp = Utils.GetSize()];

            for (int i = 0; i < r.fa32.Length; i++)
            {
                r.fa32[i] = Utils.GetRandomF32();
            }

            r.ia32 = new int[temp = Utils.GetSize()];

            for (int i = 0; i < r.ia32.Length; i++)
            {
                r.ia32[i] = Utils.GetRandomI32();
            }


            r.isa = new IntegerStruct[temp = Utils.GetSize()];

            for (int i = 0; i < r.isa.Length; i++)
            {
                r.isa[i] = IntegerStruct.GetRandomValue();
            }

            return r;
        }

        private static
            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        private static byte[] buffer;

        private static ArrayStruct desValue;

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
                desValue = bs.Desserialize<ArrayStruct>(buffer, "");


            sw.Start();
            bs.Desserialize<ArrayStruct>(buffer, "");
            sw.Stop();
        });
    }
}
