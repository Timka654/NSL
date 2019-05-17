using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test.Structs
{
    class StringStruct
    {
        [Binary(typeof(BinaryString16))]
        public string s16 { get; set; }

        //[Binary(typeof(BinaryString32))]
        //public string s32 { get; set; }

        public static StringStruct GetRandomValue()
        {
            StringStruct r = new StringStruct();

            r.s16 = Utils.GetRandomS();
            //r.s32 = Utils.GetRandomS();

            return r;
        }

        private static
            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        private static byte[] buffer;

        private static StringStruct desValue;

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
                desValue = bs.Desserialize<StringStruct>(buffer, "");


            sw.Start();
            bs.Desserialize<StringStruct>(buffer, "");
            sw.Stop();
        });
    }
}
