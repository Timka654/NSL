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
        //[Binary(typeof(BinaryInt32))]
        //public int v1 { get; set; }

        //[Binary(typeof(BinaryString32))]
        //public string s1 { get; set; }

        //[Binary(typeof(BinaryInt32))]
        //public int v2 { get; set; }

        //[Binary(typeof(BinaryInt32))]
        //public int v3 { get; set; }

        //[Binary(typeof(BinaryInt32))]
        //public int v4 { get; set; }

        //[Binary(typeof(BinaryInt32))]
        //public int v5 { get; set; }

        //[Binary(typeof(BinaryInt32))]
        //public int v6 { get; set; }

        //[Binary(typeof(InputClass))]
        //public InputClass i1 { get; set; }

        //[Binary(typeof(BinaryInt32))]
        //public int v7 { get; set; }

        //[Binary(typeof(BinaryList32<InputClass>))]
        //public List<InputClass> l1 { get; set; }

        //[Binary(typeof(BinaryList32<BinaryString32>))]
        //public List<string> l2 { get; set; }

        //[Binary(typeof(BinaryList32<BinaryInt32>))]
        //public List<int> l3 { get; set; }

        //[Binary(typeof(BinaryDictionary32<BinaryInt32, BinaryInt32>))]
        //public Dictionary<int, int> d1 { get; set; }

        //[Binary(typeof(BinaryDictionary32<BinaryInt32, InputClass>))]
        //public Dictionary<int, InputClass> d2 { get; set; }

        [Binary(typeof(BinaryArray32<BinaryInt32>))]
        public int[] l1 { get; set; }

        [Binary(typeof(BinaryArray32<InputClass>))]
        public InputClass[] l2 { get; set; }



        public void TestType()
        {
            var t1 = TypeStorage.Instance.GetTypeInfo(typeof(BinarySerializer), "");
            Tuple<int, byte[]> r;

            var inst1 = new BinarySerializer()
            {
                //i1 = new InputClass()
                //{
                //    v1 = 54
                //},
                //s1 = "53",
                //v1 = int.MaxValue,
                //v2 = int.MaxValue / 2,
                //v3 = int.MaxValue / 3,
                //v4 = int.MaxValue / 4,
                //v5 = int.MaxValue / 5,
                //v6 = int.MaxValue / 6,
                //v7 = int.MaxValue / 7,
                //l1 = new List<InputClass>()
                //{
                //    new InputClass(){ v1 = 11 },
                //    new InputClass(){ v1 = 22 },
                //    new InputClass(){ v1 = 33 },
                //    new InputClass(){ v1 = 44 },
                //    new InputClass(){ v1 = 55 },
                //},
                //l2 = new List<string>()
                //{
                //    "66",
                //    "77",
                //    "88",
                //    "99",
                //},
                //l3 = new List<int>()
                //{
                //    100,
                //    110,
                //    120,
                //    130,
                //},
                //d1 = new Dictionary<int, int>()
                //{
                //    { 00, 11 },
                //    { 22, 33 },
                //    { 44, 55 },
                //    { 66, 77 },
                //    { 88, 99 },
                //},
                //d2 = new Dictionary<int, InputClass>()
                //{
                //    { 100, new InputClass(){ v1 = 110 } },
                //    { 120, new InputClass(){ v1 = 130 } },
                //    { 140, new InputClass(){ v1 = 150 } },
                //    { 160, new InputClass(){ v1 = 170 } },
                //}
                l1 = new int[7]
                {
                    00,
                    11,
                    22,
                    33,
                    44,
                    55,
                    66
                },
                l2 = new InputClass[3]
                {
                    new InputClass{ v1 = 77 },
                    new InputClass{ v1 = 88 },
                    new InputClass{ v1 = 99 },
                }
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
