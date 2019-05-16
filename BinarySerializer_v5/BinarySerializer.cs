using BinarySerializer.DefaultTypes;
using GrEmit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    public class WithNotBinaryObjects
    {

    }

    public class InputClass
    {
        [Binary(typeof(BinaryInt32))]
        public int v1 { get; set; }
    }
    public class BinarySerializer
    {
        [Binary(typeof(BinaryInt32))]
        public int v1 { get; set; }

        [Binary(typeof(BinaryString32))]
        public string s1 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int v2 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int v3 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int v4 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int v5 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int v6 { get; set; }

        [Binary(typeof(InputClass))]
        public InputClass i1 { get; set; }

        [Binary(typeof(BinaryInt32))]
        public int v7 { get; set; }

        public void TestType()
        {
            var t1 = TypeStorage.Instance.GetTypeInfo(typeof(BinarySerializer), "");
            Tuple<int, byte[]> r;

            var inst1 = new BinarySerializer()
            {
                i1 = new InputClass()
                {
                    v1 = 54
                },
                s1 = "53",
                v1 = int.MaxValue,
                v2 = int.MaxValue / 2,
                v3 = int.MaxValue / 3,
                v4 = int.MaxValue / 4,
                v5 = int.MaxValue / 5,
                v6 = int.MaxValue / 6,
                v7 = int.MaxValue / 7,
            };
            r = t1.WriteMethod(inst1,t1);

            var q = t1.ReadMethod(r.Item2,t1);

            //for (int i = 0; i < 1000; i++)
            //{
            //    sw.Start();
            //    r = r1(inst1);
            //    sw.Stop();
            //}
            //System.Diagnostics.Debug.WriteLine($"Elapsed time : {sw.Elapsed.TotalMilliseconds}");
            //sw.Reset();
            //var inst2 = new BinaryAttribute(typeof(BinarySerializer));
            //r2(inst2);
            //for (int i = 0; i < 1000; i++)
            //{
            //    sw.Start();
            //    r = r2(inst2);
            //    sw.Stop();
            //}
            //System.Diagnostics.Debug.WriteLine($"Elapsed time : {sw.Elapsed.TotalMilliseconds}");

        }
    }
}
