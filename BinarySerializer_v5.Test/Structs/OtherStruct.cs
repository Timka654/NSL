using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    [BinarySerializer.BinaryPreCompile("default", 28)]
    public class OtherStruct : ITestStruct<OtherStruct>
    {
        [Binary(typeof(BinaryVector2))]
        [BinaryScheme("default")]
        public Vector2 v2 { get; set; }

        [Binary(typeof(BinaryVector3))]
        [BinaryScheme("default")]
        public Vector3 v3 { get; set; }

        [Binary(typeof(BinaryDateTime))]
        [BinaryScheme("default")]
        public DateTime d1 { get; set; }

        public static OtherStruct GetRndValue()
        {
            return new OtherStruct().GetRandomValue();
        }

        public override OtherStruct GetRandomValue()
        {
            OtherStruct r = new OtherStruct();

            r.v2 = Utils.GetRandomV2();
            r.v3 = Utils.GetRandomV3();
            r.d1 = Utils.GetRandomD();

            return r;
        }

        public override void streamWriteFunc(Stopwatch sw)
        {
            binaryWritedValue = base.serializedValue;
            sw.Start();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(base.serializedValue.v2.X);
            bw.Write(base.serializedValue.v2.Y);
            bw.Write(base.serializedValue.v3.X);
            bw.Write(base.serializedValue.v3.Y);
            bw.Write(base.serializedValue.v3.Z);
            bw.Write((base.serializedValue.d1 - (new DateTime(1970, 1, 1, 0, 0, 0, 0))).TotalMilliseconds);

            base.streamWriteBuffer = ms.ToArray();
            sw.Stop();
        }

        public override void streamReadFunc(Stopwatch sw)
        {
            sw.Start();

            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryReader br = new BinaryReader(ms);

            OtherStruct r = new OtherStruct();

            r.v2 = new Vector2(br.ReadSingle(), br.ReadSingle());
            r.v3 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            r.d1 = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(br.ReadDouble());

            sw.Stop();

            base.binaryReadedValue = r;
        }
    }
}
