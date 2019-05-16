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
    public class BinarySerializer
    {
        [Binary(typeof(BinaryInt32))]
        public int v1 { get; set; } = int.MaxValue;

        [Binary(typeof(BinaryString32))]
        public string s1 { get; set; } = "43";

        [Binary(typeof(BinaryInt32))]
        public int v2 { get; set; } = int.MaxValue / 2;

        [Binary(typeof(BinaryInt32))]
        public int v3 { get; set; } = int.MaxValue / 2;

        [Binary(typeof(BinaryInt32))]
        public int v4 { get; set; } = int.MaxValue / 2;

        [Binary(typeof(BinaryInt32))]
        public int v5 { get; set; } = int.MaxValue / 2;

        [Binary(typeof(BinaryInt32))]
        public int v6 { get; set; } = int.MaxValue / 2;

        public void TestType()
        {
            var t1 = TypeStorage.Instance.GetTypeInfo(typeof(BinarySerializer), "");
            //var w2 = TypeStorage.Instance.GetTypeInfo(typeof(WithNotBinaryObjects), "").WriteMethod;
            //var r2 = TypeStorage.Instance.GetTypeInfo(typeof(WithNotBinaryObjects), "").ReadMethod;


            /*ystem.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();*/

            Tuple<int, byte[]> r;

            var inst1 = new BinarySerializer();
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
