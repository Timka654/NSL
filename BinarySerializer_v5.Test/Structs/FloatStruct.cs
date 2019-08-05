using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    [BinarySerializer.BinaryPreCompile("default", 12)]
    public class FloatStruct : ITestStruct<FloatStruct>
    {
        [Binary(typeof(BinaryFloat32))]
        [BinaryScheme("default")]
        public float f32 { get; set; }

        [Binary(typeof(BinaryFloat64))]
        [BinaryScheme("default")]
        public double f64 { get; set; }

        public static FloatStruct GetRndValue()
        {
            return new FloatStruct().GetRandomValue();
        }

        public override FloatStruct GetRandomValue()
        {
            FloatStruct r = new FloatStruct();

            r.f32 = Utils.GetRandomF32();
            r.f64 = Utils.GetRandomF64();

            return r;
        }

        public override void streamWriteFunc(Stopwatch sw)
        {
            binaryWritedValue = base.serializedValue;
            sw.Start();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(base.serializedValue.f32);
            bw.Write(base.serializedValue.f64);

            base.streamWriteBuffer = ms.ToArray();
            sw.Stop();
        }

        public override void streamReadFunc(Stopwatch sw)
        {
            sw.Start();

            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryReader br = new BinaryReader(ms);

            FloatStruct r = new FloatStruct();

            r.f32 = br.ReadSingle();
            r.f64 = br.ReadDouble();

            sw.Stop();

            base.binaryReadedValue = r;
        }
    }
}
