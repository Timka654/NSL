using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class DictionaryStruct : ITestStruct<DictionaryStruct>
    {
        [Binary(typeof(BinaryDictionary16<BinaryFloat32, BinaryInt32>))]
        public Dictionary<float, int> nulld16 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryFloat32>))]
        public Dictionary<int, float> nulld32 { get; set; }

        [Binary(typeof(BinaryDictionary16<BinaryFloat32, BinaryInt32>))]
        public Dictionary<float, int> emptyd16 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryFloat32>))]
        public Dictionary<int, float> emptyd32 { get; set; }

        [Binary(typeof(BinaryDictionary16<BinaryFloat32,BinaryInt32>))]
        public Dictionary<float,int> d16 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32,BinaryFloat32>))]
        public Dictionary<int, float> d32 { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryInt32>))]
        public Dictionary<int, int> nv1 { get; set; }

        [Binary(typeof(BinaryDictionary16<BinaryInt32, IntegerStruct>))]
        public Dictionary<int, IntegerStruct> isd { get; set; }

        [Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryInt32>))]
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
    }
}
