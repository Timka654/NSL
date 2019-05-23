using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class ArrayStruct : ITestStruct<ArrayStruct>
    {
        [Binary(typeof(BinaryArray16<BinaryString16>))]
        public string[] nullsv1 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        public int[] nullia32 { get; set; }

        [Binary(typeof(BinaryArray16<BinaryString16>))]
        public string[] emptysv1 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        public int[] emptyia32 { get; set; }


        [Binary(typeof(BinaryArray16<BinaryFloat32>))]
        public float[] fa32 { get; set; }

        [Binary(typeof(BinaryArray16<BinaryString16>))]
        public string[] sv1 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        public int[] ia32 { get; set; }

        [Binary(typeof(BinaryArray16<IntegerStruct>))]
        public IntegerStruct[] isa { get; set; }

        public static ArrayStruct GetRndValue()
        {
            return new ArrayStruct().GetRandomValue();
        }

        public override ArrayStruct GetRandomValue()
        {
            var r = new ArrayStruct();

            r.emptyia32 = new int[0];
            r.emptysv1 = new string[0];

            r.fa32 = new float[Utils.GetSize()];

            for (int i = 0; i < r.fa32.Length; i++)
            {
                r.fa32[i] = Utils.GetRandomF32();
            }

            r.ia32 = new int[Utils.GetSize()];

            for (int i = 0; i < r.ia32.Length; i++)
            {
                r.ia32[i] = Utils.GetRandomI32();
            }


            r.isa = new IntegerStruct[Utils.GetSize()];

            for (int i = 0; i < r.isa.Length; i++)
            {
                r.isa[i] = IntegerStruct.GetRndValue();
            }

            r.sv1 = new string[Utils.GetSize()];

            for (int i = 0; i < r.sv1.Length; i++)
            {
                r.sv1[i] = Utils.GetRandomS();
            }

            return r;
        }

        public override void streamWriteFunc(Stopwatch sw)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);




            base.streamWriteBuffer = ms.ToArray();
        }

        public override void streamReadFunc(Stopwatch sw)
        {
            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryWriter bw = new BinaryWriter(ms);
        }
    }
}
