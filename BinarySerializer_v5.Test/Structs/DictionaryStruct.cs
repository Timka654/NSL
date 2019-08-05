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
    public class DictionaryStruct : ITestStruct<DictionaryStruct>
    {
        [Binary(typeof(BinaryDictionary16<BinaryFloat32, BinaryInt32>))]
        [BinaryScheme("default")]
        public Dictionary<float, int> nulld16 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryFloat32>))]
        [BinaryScheme("default")]
        public Dictionary<int, float> nulld32 { get; set; }

        [Binary(typeof(BinaryDictionary16<BinaryFloat32, BinaryInt32>))]
        [BinaryScheme("default")]
        public Dictionary<float, int> emptyd16 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryFloat32>))]
        [BinaryScheme("default")]
        public Dictionary<int, float> emptyd32 { get; set; }

        [Binary(typeof(BinaryDictionary16<BinaryFloat32,BinaryInt32>))]
        [BinaryScheme("default")]
        public Dictionary<float,int> d16 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32,BinaryFloat32>))]
        [BinaryScheme("default")]
        public Dictionary<int, float> d32 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryInt32>))]
        [BinaryScheme("default")]
        public Dictionary<int, int> nv1 { get; set; }

        [Binary(typeof(BinaryDictionary16<BinaryInt32, IntegerStruct>))]
        [BinaryScheme("default")]
        public Dictionary<int, IntegerStruct> isd { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryInt32>))]
        [BinaryScheme("default")]
        public Dictionary<int, int> nv2 { get; set; }

        public static DictionaryStruct GetRndValue()
        {
            return new DictionaryStruct().GetRandomValue();
        }

        public override DictionaryStruct GetRandomValue()
        {
            DictionaryStruct r = new DictionaryStruct();

            r.emptyd16 = new Dictionary<float, int>();
            r.emptyd32 = new Dictionary<int, float>();

            r.d16 = new Dictionary<float, int>();

            for (int i = 0; i < Utils.GetSize(); i++)
            {
                r.d16.Add(Utils.GetRandomF32(),Utils.GetRandomI32());
            }

            r.d32 = new Dictionary<int, float>();

            for (int i = 0; i < Utils.GetSize(); i++)
            {
                r.d32.Add(Utils.GetRandomI32(), Utils.GetRandomF32());
            }

            r.isd = new Dictionary<int, IntegerStruct>();

            for (int i = 0; i < Utils.GetSize(); i++)
            {
                r.isd.Add(Utils.GetRandomI32(), IntegerStruct.GetRndValue());
            }

            return r;
        }

        private bool WriteDictionaryHeader32(BinaryWriter bw, IDictionary arr)
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

        private bool WriteDictionaryHeader16(BinaryWriter bw, IDictionary arr)
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

        private Tuple<bool, int> ReadDictionaryHeader32(BinaryReader br)
        {
            if (br.ReadBoolean())
                return new Tuple<bool, int>(false, 0);

            return new Tuple<bool, int>(true, br.ReadInt32());
        }

        private Tuple<bool, short> ReadDictionaryHeader16(BinaryReader br)
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

            if (WriteDictionaryHeader16(bw, base.binaryWritedValue.nulld16))
            {
                foreach (var item in base.binaryWritedValue.nulld16)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader32(bw, base.binaryWritedValue.nulld32))
            {
                foreach (var item in base.binaryWritedValue.nulld32)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader16(bw, base.binaryWritedValue.emptyd16))
            {
                foreach (var item in base.binaryWritedValue.emptyd16)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader32(bw, base.binaryWritedValue.emptyd32))
            {
                foreach (var item in base.binaryWritedValue.emptyd32)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader16(bw, base.binaryWritedValue.d16))
            {
                foreach (var item in base.binaryWritedValue.d16)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader32(bw, base.binaryWritedValue.d32))
            {
                foreach (var item in base.binaryWritedValue.d32)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader32(bw, base.binaryWritedValue.nv1))
            {
                foreach (var item in base.binaryWritedValue.nv1)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
                }
            }

            if (WriteDictionaryHeader16(bw, base.binaryWritedValue.isd))
            {
                foreach (var item in base.binaryWritedValue.isd)
                {
                    bw.Write(item.Key);
                    IntegerStruct.Write(bw, item.Value);
                }
            }

            if (WriteDictionaryHeader32(bw, base.binaryWritedValue.nv2))
            {
                foreach (var item in base.binaryWritedValue.nv2)
                {
                    bw.Write(item.Key);
                    bw.Write(item.Value);
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
            DictionaryStruct r = new DictionaryStruct();

            var h16 = ReadDictionaryHeader16(br);
            if (h16.Item1)
            {
                r.nulld16 = new Dictionary<float, int>();
                for (int i = 0; i < h16.Item2; i++)
                {
                    r.nulld16.Add(br.ReadSingle(), br.ReadInt32());
                }
            }

            var h32 = ReadDictionaryHeader32(br);
            if (h32.Item1)
            {
                for (int i = 0; i < h32.Item2; i++)
                {
                    r.nulld32.Add(br.ReadInt32(), br.ReadSingle());
                }
            }

            h16 = ReadDictionaryHeader16(br);
            if (h16.Item1)
            {
                r.emptyd16 = new Dictionary<float, int>();
                for (int i = 0; i < h16.Item2; i++)
                {
                    r.emptyd16.Add(br.ReadSingle(), br.ReadInt32());
                }
            }

            h32 = ReadDictionaryHeader32(br);
            if (h32.Item1)
            {
                r.emptyd32 = new Dictionary<int, float>();
                for (int i = 0; i < h32.Item2; i++)
                {
                    r.emptyd32.Add(br.ReadInt32(), br.ReadSingle());
                }
            }

            h16 = ReadDictionaryHeader16(br);
            if (h16.Item1)
            {
                r.d16 = new Dictionary<float, int>();
                for (int i = 0; i < h16.Item2; i++)
                {
                    r.d16.Add(br.ReadSingle(), br.ReadInt32());
                }
            }

            h32 = ReadDictionaryHeader32(br);
            if (h32.Item1)
            {
                r.d32 = new Dictionary<int, float>();
                for (int i = 0; i < h32.Item2; i++)
                {
                    r.d32.Add(br.ReadInt32(), br.ReadSingle());
                }
            }

            h32 = ReadDictionaryHeader32(br);
            if (h32.Item1)
            {
                r.nv1 = new Dictionary<int, int>();
                for (int i = 0; i < h32.Item2; i++)
                {
                    r.nv1.Add(br.ReadInt32(), br.ReadInt32());
                }
            }

            h16 = ReadDictionaryHeader16(br);
            if (h16.Item1)
            {
                r.isd = new Dictionary<int, IntegerStruct>();
                for (int i = 0; i < h16.Item2; i++)
                {
                    r.isd.Add(br.ReadInt32(), IntegerStruct.Read(br));
                }
            }

            h32 = ReadDictionaryHeader32(br);
            if (h32.Item1)
            {
                r.nv2 = new Dictionary<int, int>();
                for (int i = 0; i < h32.Item2; i++)
                {
                    r.nv2.Add(br.ReadInt32(), br.ReadInt32());
                }
            }

            sw.Stop();
            if (binaryReadedValue == null)
                binaryReadedValue = r;
        }
    }
}
