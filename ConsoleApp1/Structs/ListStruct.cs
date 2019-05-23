using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class ListStruct : ITestStruct<ListStruct>
    {
        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        public List<float> nullfl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        public List<int> nullil32 { get; set; }

        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        public List<float> emptyfl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        public List<int> emptyil32 { get; set; }

        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        public List<float> fl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        public List<int> il32 { get; set; }

        [Binary(typeof(BinaryList16<IntegerStruct>))]
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
    }
}
