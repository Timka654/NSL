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
    public class ArrayStruct : ITestStruct<ArrayStruct>
    {
        [Binary(typeof(BinaryArray16<BinaryString16>))]
        [BinaryScheme("default")]
        public string[] nullsv1 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        [BinaryScheme("default")]
        public int[] nullia32 { get; set; }

        [Binary(typeof(BinaryArray16<BinaryString16>))]
        [BinaryScheme("default")]
        public string[] emptysv1 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        [BinaryScheme("default")]
        public int[] emptyia32 { get; set; }

        [Binary(typeof(BinaryArray16<BinaryFloat32>))]
        [BinaryScheme("default")]
        public float[] fa32 { get; set; }

        [Binary(typeof(BinaryArray16<BinaryString16>))]
        [BinaryScheme("default")]
        public string[] sv1 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        [BinaryScheme("default")]
        public int[] ia32 { get; set; }

        [Binary(typeof(BinaryArray16<IntegerStruct>))]
        [BinaryScheme("default")]
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

        private bool WriteArrayHeader32(BinaryWriter bw, Array arr)
        {
            if (arr == null)
            {
                bw.Write(true);
                return false;
            }

            bw.Write(false);
            bw.Write(arr.Length);
            return true;
        }

        private bool WriteArrayHeader16(BinaryWriter bw, Array arr)
        {
            if (arr == null)
            {
                bw.Write(true);
                return false;
            }

            bw.Write(false);
            bw.Write((short)arr.Length);
            return true;
        }

        private Tuple<bool, int> ReadArrayHeader32(BinaryReader br)
        {
            if (br.ReadBoolean())
                return new Tuple<bool, int>(false, 0);

            return new Tuple<bool, int>(true, br.ReadInt32());
        }

        private Tuple<bool, short> ReadArrayHeader16(BinaryReader br)
        {
            if (br.ReadBoolean())
                return new Tuple<bool, short>(false, 0);

            return new Tuple<bool, short>(true, br.ReadInt16());
        }

        public override void streamWriteFunc(Stopwatch sw)
        {
            base.binaryWritedValue = base.serializedValue;

            sw.Start();

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            if (WriteArrayHeader16(bw, base.binaryWritedValue.nullsv1))
            {
                foreach (var item in base.binaryWritedValue.nullsv1)
                {
                    StringStruct.WriteString16(bw, item);
                }
            }

            if (WriteArrayHeader32(bw, base.binaryWritedValue.nullia32))
            {
                foreach (var item in base.binaryWritedValue.nullia32)
                {
                    bw.Write(item);
                }
            }

            if (WriteArrayHeader16(bw, base.binaryWritedValue.emptysv1))
            {
                foreach (var item in base.binaryWritedValue.emptysv1)
                {
                    StringStruct.WriteString16(bw, item);
                }
            }

            if (WriteArrayHeader32(bw, base.binaryWritedValue.emptyia32))
            {
                foreach (var item in base.binaryWritedValue.emptyia32)
                {
                    bw.Write(item);
                }
            }

            if (WriteArrayHeader16(bw, base.binaryWritedValue.fa32))
            {
                foreach (var item in base.binaryWritedValue.fa32)
                {
                    bw.Write(item);
                }
            }

            if (WriteArrayHeader16(bw, base.binaryWritedValue.sv1))
            {
                foreach (var item in base.binaryWritedValue.sv1)
                {
                    StringStruct.WriteString16(bw, item);
                }
            }

            if (WriteArrayHeader32(bw, base.binaryWritedValue.ia32))
            {
                foreach (var item in base.binaryWritedValue.ia32)
                {
                    bw.Write(item);
                }
            }

            if (WriteArrayHeader16(bw, base.binaryWritedValue.isa))
            {
                foreach (var item in base.binaryWritedValue.isa)
                {
                    IntegerStruct.Write(bw, item);
                }
            }

            sw.Stop();

            base.streamWriteBuffer = ms.ToArray();
        }

        public override void streamReadFunc(Stopwatch sw)
        {
            sw.Start();

            MemoryStream ms = new MemoryStream(base.streamWriteBuffer);
            BinaryReader br = new BinaryReader(ms);

            var r = new ArrayStruct();

            // nullsv1, int16
            var ex16 = ReadArrayHeader16(br);

            if (ex16.Item1)
            {
                r.nullsv1 = new string[ex16.Item2];
                for (int i = 0; i < ex16.Item2; i++)
                {
                    r.nullsv1[i] = StringStruct.ReadString16(br);
                }
            }

            //nullia32, int32
            var ex32 = ReadArrayHeader32(br);

            if (ex32.Item1)
            {
                r.nullia32 = new int[ex32.Item2];

                for (int i = 0; i < ex32.Item2; i++)
                {
                    r.nullia32[i] = br.ReadInt32();
                }
            }

            //emptysv1, int16
            ex16 = ReadArrayHeader16(br);

            if (ex16.Item1)
            {
                r.emptysv1 = new string[ex16.Item2];
                for (int i = 0; i < ex16.Item2; i++)
                {
                    r.emptysv1[i] = StringStruct.ReadString16(br);
                }
            }

            //emptyia32, int32
            ex32 = ReadArrayHeader32(br);

            if (ex32.Item1)
            {
                r.emptyia32 = new int[ex32.Item2];

                for (int i = 0; i < ex32.Item2; i++)
                {
                    r.emptyia32[i] = br.ReadInt32();
                }
            }

            //fa32, int16
            ex16 = ReadArrayHeader16(br);

            if (ex16.Item1)
            {
                r.fa32 = new float[ex16.Item2];
                for (int i = 0; i < ex16.Item2; i++)
                {
                    r.fa32[i] = br.ReadSingle();
                }
            }

            //sv1, int16
            ex16 = ReadArrayHeader16(br);

            if (ex16.Item1)
            {
                r.sv1 = new string[ex16.Item2];
                for (int i = 0; i < ex16.Item2; i++)
                {
                    r.sv1[i] = StringStruct.ReadString16(br);
                }
            }

            ex32 = ReadArrayHeader32(br);

            if (ex32.Item1)
            {
                r.ia32 = new int[ex32.Item2];
                for (int i = 0; i < ex32.Item2; i++)
                {
                    r.ia32[i] = br.ReadInt32();
                }
            }

            ex16 = ReadArrayHeader16(br);

            if (ex16.Item1)
            {
                r.isa = new IntegerStruct[ex16.Item2];
                for (int i = 0; i < ex16.Item2; i++)
                {
                    r.isa[i] = IntegerStruct.Read(br);
                }
            }

            sw.Stop();

            base.binaryReadedValue = r;
        }
    }
}
