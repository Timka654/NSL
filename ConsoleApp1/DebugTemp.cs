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
        public class TStruct
        {
            [Binary(typeof(BinaryBool))]
            public bool b { get; set; }

            [Binary(typeof(BinaryString16))]
            public string s { get; set; }


            [Binary(typeof(BinaryTimeSpan))]
            public TimeSpan ts { get; set; }
            //[Binary(typeof(BinaryVector2))]
            //public Vector2 v2 { get; set; }

            //[Binary(typeof(BinaryVector3))]
            //public Vector3 v3 { get; set; }

            //[Binary(typeof(BinaryDateTime))]
            //public DateTime v5 { get; set; }

            public static void Debug()
            {
                TStruct ts = new TStruct()
                {
                    s = "111",
                    ts = new TimeSpan(23, 11, 44, 55, 44)
                        //v2 = new Vector2(11, 55),
                        //v3 = new Vector3(44, 66, 99),
                        //v5 = new DateTime(2011, 4, 10, 5, 7, 29, 9, DateTimeKind.Utc)
                };
            
                //ts.v.X = 35;
                //ts.v.Y = 43;


                BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

                var buf = bs.Serialize("", ts);
                int offset = 0;

                var res = bs.Deserialize<TStruct>("", buf,ref offset);
            }
        }
    }
}
