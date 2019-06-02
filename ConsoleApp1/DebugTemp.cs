using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BinarySerializer_v5.Test
{
    public class DebugTemp
    {

        public class RecurciveError
        {
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("default")]
            public int q { get; set; } = 12;


            [Binary(typeof(BinaryList16<tempStruct>))]
            [BinaryScheme("default")]
            public List<tempStruct> t { get; set; }
        }

        public class tempStruct
        {

            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("default")]
            public int q { get; set; } = 22;
        }

        public class TStruct
        {
            public static void Debug()
            {

                RecurciveError r = new RecurciveError();
                r.t = new List<tempStruct>();
                for (int i = 0; i < 5; i++)
                {
                    r.t.Add(new tempStruct());
                }


                BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

                var buf3 = bs.Serialize("default", r);

            }
        }
    }
}
