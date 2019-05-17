using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    public class ListStruct
    {
        [Binary(typeof(BinaryList16<BinaryFloat32>))]
        public List<float> fl32 { get; set; }

        [Binary(typeof(BinaryList32<BinaryInt32>))]
        public List<int> il32 { get; set; }

        [Binary(typeof(BinaryList16<IntegerStruct>))]
        public List<IntegerStruct> isl { get; set; }

        public static ListStruct GetRandomValue()
        {
            ListStruct r = new ListStruct();

            r.fl32 = new List<float>();

            for (int i = 0; i < Utils.GetRandomI32(); i++)
            {
                r.fl32.Add(Utils.GetRandomF32());
            }

            r.il32 = new List<int>();

            for (int i = 0; i < Utils.GetRandomI32(); i++)
            {
                r.il32.Add(Utils.GetRandomI32());
            }

            r.isl = new List<IntegerStruct>();

            for (int i = 0; i < Utils.GetRandomI32(); i++)
            {
                r.isl.Add(IntegerStruct.GetRandomValue());
            }

            return r;
        }

        private static
            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        private static byte[] buffer;

        private static ListStruct desValue;

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
                desValue = bs.Desserialize<ListStruct>(buffer, "");


            sw.Start();
            bs.Desserialize<ListStruct>(buffer, "");
            sw.Stop();
        });
    }
}
