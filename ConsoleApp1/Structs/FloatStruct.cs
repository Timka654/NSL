using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class FloatStruct
    {
        [Binary(typeof(BinaryFloat32))]
        public float f32 { get; set; }

        [Binary(typeof(BinaryFloat32))]
        public double f64 { get; set; }

        public static FloatStruct GetRandomValue()
        {
            FloatStruct r = new FloatStruct();

            r.f32 = Utils.GetRandomF32();
            r.f64 = Utils.GetRandomF64();

            return r;
        }

        private static
            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        private static byte[] buffer;

        private static FloatStruct desValue;

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
                desValue = bs.Desserialize<FloatStruct>(buffer, "");


            sw.Start();
            bs.Desserialize<FloatStruct>(buffer, "");
            sw.Stop();
        });
    }
}
