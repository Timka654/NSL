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

            [Binary(typeof(BinaryList16<SubRecurciveError>))]
            [BinaryScheme("default")]
            public List<SubRecurciveError> l { get; set; }
        }


        public class SubRecurciveError
        {
            [Binary(typeof(RecurciveError))]
            [BinaryScheme("default2")]
            public RecurciveError err { get; set; }
        }

        public class TStruct
        {
            //[Binary(typeof(BinaryBool))]
            //public bool b { get; set; }

            //[Binary(typeof(BinaryString16))]
            //public string s { get; set; }


            //[Binary(typeof(BinaryTimeSpan))]
            //public TimeSpan ts { get; set; }


            [Binary(typeof(BinaryNullableInt32))]
            [BinaryScheme("default")]
            public int? ni32 { get; set; }

            [Binary(typeof(BinaryNullableDateTime))]
            [BinaryScheme("default")]
            public DateTime? ndt { get; set; }

            [Binary(typeof(BinaryNullableTimeSpan))]
            [BinaryScheme("default")]
            public TimeSpan? nts { get; set; }

            [Binary(typeof(BinaryNullableVector2))]
            [BinaryScheme("default")]
            public Vector2? nv2 { get; set; }

            [Binary(typeof(BinaryNullableVector3))]
            [BinaryScheme("default")]
            public Vector3? nv3 { get; set; }

            //[Binary(typeof(BinaryVector2))]
            //public Vector2 v2 { get; set; }

            //[Binary(typeof(BinaryVector3))]
            //public Vector3 v3 { get; set; }

            //[Binary(typeof(BinaryDateTime))]
            //public DateTime v5 { get; set; }

            public static void Debug()
            {

                RecurciveError r = new RecurciveError();
                r.l = new List<SubRecurciveError>();
                for (int i = 0; i < 5; i++)
                {
                    r.l.Add(new SubRecurciveError() { });
                }


                BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

                var buf3 = bs.Serialize("default", r);


                TStruct ts1 = new TStruct()
                {
                    ni32 = 10,
                    ndt = new DateTime(2011, 11, 11),
                    nts = new TimeSpan(33, 55, 11),
                    nv2 = new Vector2(88, 99),
                    nv3 = new Vector3(1111, 1212, 1313)
                    //s = "111",
                    //ts = new TimeSpan(23, 11, 44, 55, 44)
                    //v2 = new Vector2(11, 55),
                    //v3 = new Vector3(44, 66, 99),
                    //v5 = new DateTime(2011, 4, 10, 5, 7, 29, 9, DateTimeKind.Utc)
                };


                var buf1 = bs.Serialize("default", ts1);
                int offset = 0;

                var res1 = bs.Deserialize<TStruct>("default", buf1, ref offset);

                offset = 0;

                TStruct ts2 = new TStruct()
                {
                };

                var buf2 = bs.Serialize("default", ts2);

                var res2 = bs.Deserialize<TStruct>("default", buf2, ref offset);
            }
        }
    }
}
