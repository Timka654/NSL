using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    [BinarySerializer.BinaryPreCompile("default", 500)]
    public class ListStruct : ITestStruct<ListStruct>
    {
        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        [BinaryScheme("default")]
        public List<float> nullfl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        [BinaryScheme("default")]
        public List<int> nullil32 { get; set; }

        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        [BinaryScheme("default")]
        public List<float> emptyfl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        [BinaryScheme("default")]
        public List<int> emptyil32 { get; set; }

        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        [BinaryScheme("default")]
        public List<float> fl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        [BinaryScheme("default")]
        public List<int> il32 { get; set; }

        [Binary(typeof(BinaryList16<IntegerStruct>))]
        [BinaryScheme("default")]
        public List<IntegerStruct> isl { get; set; }

        public static ListStruct GetRndValue()
        {
            return new ListStruct().GetRandomValue();
        }

        public override ListStruct GetRandomValue()
        {
            ListStruct r = new ListStruct();

            r.emptyfl32 = new List<float>();
            r.emptyil32 = new List<int>();

            r.fl32 = new List<float>();

            for (int i = 0; i < Utils.GetSize(); i++)
            {
                r.fl32.Add(Utils.GetRandomF32());
            }

            r.il32 = new List<int>();

            for (int i = 0; i < Utils.GetSize(); i++)
            {
                r.il32.Add(Utils.GetRandomI32());
            }

            r.isl = new List<IntegerStruct>();

            for (int i = 0; i < Utils.GetSize(); i++)
            {
                r.isl.Add(IntegerStruct.GetRndValue());
            }

            return r;
        }

        private bool WriteListHeader32(BinaryWriter bw, IList arr)
        {
            if (arr == null)
            {
                bw.Write(true);
                return false;
            }

            bw.Write(false);
            bw.Write(arr.Count);
            return true;
        }

        private bool WriteListHeader16(BinaryWriter bw, IList arr)
        {
            if (arr == null)
            {
                bw.Write(true);
                return false;
            }

            bw.Write(false);
            bw.Write((short)arr.Count);
            return true;
        }

        private Tuple<bool, int> ReadListHeader32(BinaryReader br)
        {
            if (br.ReadBoolean())
                return new Tuple<bool, int>(false, 0);

            return new Tuple<bool, int>(true, br.ReadInt32());
        }

        private Tuple<bool, short> ReadListHeader16(BinaryReader br)
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

            if (WriteListHeader16(bw, base.binaryWritedValue.nullfl32))
            {
                foreach (var item in base.binaryWritedValue.nullfl32)
                {
                    bw.Write(item);
                }
            }

            if (WriteListHeader32(bw, base.binaryWritedValue.nullil32))
            {
                foreach (var item in base.binaryWritedValue.nullil32)
                {
                    bw.Write(item);
                }
            }

            if (WriteListHeader16(bw, base.binaryWritedValue.emptyfl32))
            {
                foreach (var item in base.binaryWritedValue.emptyfl32)
                {
                    bw.Write(item);
                }
            }

            if (WriteListHeader32(bw, base.binaryWritedValue.emptyil32))
            {
                foreach (var item in base.binaryWritedValue.emptyil32)
                {
                    bw.Write(item);
                }
            }

            if (WriteListHeader16(bw, base.binaryWritedValue.fl32))
            {
                foreach (var item in base.binaryWritedValue.fl32)
                {
                    bw.Write(item);
                }
            }

            if (WriteListHeader32(bw, base.binaryWritedValue.il32))
            {
                foreach (var item in base.binaryWritedValue.il32)
                {
                    bw.Write(item);
                }
            }

            if (WriteListHeader16(bw, base.binaryWritedValue.isl))
            {
                foreach (var item in base.binaryWritedValue.isl)
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
            ListStruct r = new ListStruct();


            var h16 = ReadListHeader16(br);

            if (h16.Item1)
            {
                r.nullfl32 = new List<float>();

                for (int i = 0; i < h16.Item2; i++)
                {
                    r.nullfl32.Add(br.ReadSingle());
                }
            }

            var h32 = ReadListHeader32(br);

            if (h32.Item1)
            {
                r.nullil32 = new List<int>();

                for (int i = 0; i < h32.Item2; i++)
                {
                    r.nullil32.Add(br.ReadInt32());
                }
            }

            h16 = ReadListHeader16(br);

            if (h16.Item1)
            {
                r.emptyfl32 = new List<float>();

                for (int i = 0; i < h16.Item2; i++)
                {
                    r.emptyfl32.Add(br.ReadSingle());
                }
            }

            h32 = ReadListHeader32(br);

            if (h32.Item1)
            {
                r.emptyil32 = new List<int>();

                for (int i = 0; i < h32.Item2; i++)
                {
                    r.emptyil32.Add(br.ReadInt32());
                }
            }

            h16 = ReadListHeader16(br);

            if (h16.Item1)
            {
                r.fl32 = new List<float>();

                for (int i = 0; i < h16.Item2; i++)
                {
                    r.fl32.Add(br.ReadSingle());
                }
            }

            h32 = ReadListHeader32(br);

            if (h32.Item1)
            {
                r.il32 = new List<int>();

                for (int i = 0; i < h32.Item2; i++)
                {
                    r.il32.Add(br.ReadInt32());
                }
            }

            h16 = ReadListHeader16(br);

            if (h16.Item1)
            {
                r.isl = new List<IntegerStruct>();

                for (int i = 0; i < h16.Item2; i++)
                {
                    r.isl.Add(IntegerStruct.Read(br));
                }
            }

            sw.Stop();
            if (binaryReadedValue == null)
                binaryReadedValue = r;
        }
    }
}
