using BinarySerializer;
using BinarySerializerTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BinarySerializer.DefaultTypes;
using BinarySerializer.DefaultTypesV5;

namespace BinarySerializerTest
{
    class Program
    {

        static void Main(string[] args)
        {

            BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();


            bs.TestType();


            /*

            BinarySerializer.Tests.TestData td = new BinarySerializer.Tests.TestData();

            BinarySerializer.Tests.Test.Compare = false;

            BinarySerializer.Tests.Test.Iterations = 1000;

            BinarySerializer.Tests.Test.FullTest(td.LoadFULL(false));
            */
            Console.ReadKey();
        }
        private static Random rand = new Random();

        static string GetRandomString()
        {
            string r = "";
            int len = rand.Next() % 255;

            for (int i = 0; i < len; i++)
            {
                r += (char)(rand.Next() % 64) + 5;
            }

            return r;
        }

        public class StringTest
        {
            [Binary(typeof(BinaryString), TypeSize = 10, ArraySize = 55)]
            public string[] Values { get;set; }
        }

        private static byte[] strToBytes(string text, int len)
        {
            byte[] r = new byte[len];
            int br = Encoding.UTF8.GetByteCount(text);
            Array.Copy(Encoding.UTF8.GetBytes(text), r, br < len? br:len);
            return r;
        }

        static PrimitiveTestClass GeneratePrimitiveData(Random rand)
        {
            //len: 140

            PrimitiveTestClass r = new PrimitiveTestClass();
            r.i0 = 1;
            r.i1 = 2;
            r.i2 = 3;
            r.i3 = 4;
            r.i4 = long.MaxValue;
            r.i5 = ulong.MaxValue;
            r.f0 = 1.0f;
            r.f1 = 2.0f;
            r.len = 35;
            r.s0 = GetRandomString();
            r.s1 = GetRandomString();
            r.s2 = GetRandomString();

            return r;
        }

        static TypeTestClass GenerateTypeData(Random rand)
        {
            //len: 560
            TypeTestClass r = new TypeTestClass();

            r.c0 = GeneratePrimitiveData(rand);

            r.c1 = GeneratePrimitiveData(rand);

            r.c2 = GeneratePrimitiveData(rand);

            r.c3 = GeneratePrimitiveData(rand);

            return r;
        }

        static PrimitiveArrayTestClass GeneratePrimitiveArrayData(Random rand)
        {
            PrimitiveArrayTestClass r = new PrimitiveArrayTestClass();

            r.ia0 = new int[rand.Next() % 15];

            for (int i = 0; i < r.ia0.Length; i++)
            {
                r.ia0[i] = rand.Next() % 200;
            }

            r.ia1 = new int[rand.Next() % 15];

            for (int i = 0; i < r.ia1.Length; i++)
            {
                r.ia1[i] = rand.Next() % 200;
            }

            r.len = 5;

            r.ia2 = new int[rand.Next() % 10];

            for (int i = 0; i < r.ia2.Length; i++)
            {
                r.ia2[i] = i;
            }

            return r;
        }

        static TypeArrayTestClass GenerateTypeArrayData(Random rand)
        {
            // len : 14004
            TypeArrayTestClass r = new TypeArrayTestClass();

            r.arr0 = new TypeTestClass[rand.Next() % 15 + 1];

            for (int i = 0; i < rand.Next() % r.arr0.Length; i++)
            {
                r.arr0[i] = GenerateTypeData(rand);
            }

            r.len = 15;

            r.arr1 = new TypeTestClass[15];

            for (int i = 0; i < r.len; i++)
            {
                r.arr1[i] = GenerateTypeData(rand);
            }

            return r;
        }
    }
}
