using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinarySerializer.Tests
{
    public class TestData
    {
        #region Structure

        [Serializable]
        public class TestStructure
        {
            [BinaryAttribute(typeof(TestStructureNumeric))]
            public TestStructureNumeric t1 { get; set; }
            [BinaryAttribute(typeof(TestStructureArray))]
            public TestStructureArray t2 { get; set; }
            [BinaryAttribute(typeof(TestStructureString))]
            public TestStructureString t3 { get; set; }

            [BinaryAttribute(typeof(TestStructureNumeric), ArraySize = 5)]
            public TestStructureNumeric[] t4 { get; set; }
            [BinaryAttribute(typeof(TestStructureArray), ArraySize = 5)]
            public TestStructureArray[] t5 { get; set; }
            [BinaryAttribute(typeof(TestStructureString), ArraySize = 5)]
            public TestStructureString[] t6 { get; set; }

            [BinaryAttribute(typeof(BinaryInt32))]
            public int tlen { get; set; }

            public List<TestStructureNumeric> t7 { get; set; }

            [BinaryAttribute(typeof(TestStructureNumeric), ArraySizeName = "tlen")]
            public TestStructureNumeric[] _t7 { get { return t7.ToArray(); } set { t7 = value.ToList(); } }

            [BinaryAttribute(typeof(TestStructureArray), ArraySizeName = "tlen")]
            public TestStructureArray[] t8 { get; set; }
            [BinaryAttribute(typeof(TestStructureString), ArraySizeName = "tlen")]
            public TestStructureString[] t9 { get; set; }
            [BinaryAttribute(typeof(BinaryInt32))]
            public int glen { get; set; }
        }

        [Serializable]
        public class TestStructureNumeric
        {
            [BinaryAttribute(typeof(BinaryInt16))]
            public short t1 { get; set; }
            [BinaryAttribute(typeof(BinaryInt32))]
            public int t2 { get; set; }
            [BinaryAttribute(typeof(BinaryInt64))]
            public long t3 { get; set; }
            [BinaryAttribute(typeof(BinaryFloat32))]
            public float t4 { get; set; }
            [BinaryAttribute(typeof(BinaryFloat64))]
            public double t5 { get; set; }
            [BinaryAttribute(typeof(BinaryByte))]
            public byte t6 { get; set; }
        }

        [Serializable]
        public class TestStructureArray
        {
            [BinaryAttribute(typeof(BinaryInt32))]
            public int len { get; set; }
            [BinaryAttribute(typeof(BinaryFloat32), ArraySize = 10)]
            public float[] t1 { get; set; }
            [BinaryAttribute(typeof(BinaryFloat32), ArraySizeName = "len")]
            public float[] t2 { get; set; }
            [BinaryAttribute(typeof(BinaryInt64), ArraySize = 10)]
            public long[] t3 { get; set; }
            [BinaryAttribute(typeof(BinaryInt64), ArraySizeName = "len")]
            public long[] t4 { get; set; }
            [BinaryAttribute(typeof(TestStructureNumeric), ArraySizeName = "len")]
            public TestStructureNumeric[] _t7 { get; set; }
        }

        [Serializable]
        public class TestStructureString
        {
            [BinaryAttribute(typeof(BinaryInt32))]
            public int len { get; set; }

            [BinaryAttribute(typeof(BinaryString), TypeSize = 10)]
            public string t1 { get; set; }
            [BinaryAttribute(typeof(BinaryString), TypeSizeName = "len", ArraySize = 55)]
            public string[] t2 { get; set; }


            [BinaryAttribute(typeof(BinaryString), TypeSizeName = "len")]
            public string t3 { get; set; }
            [BinaryAttribute(typeof(BinaryString), TypeSize = 10, ArraySizeName = "len")]
            public string[] t4 { get; set; }

            [BinaryAttribute(typeof(BinaryString), TypeSize = 10, ArraySize = 55)]
            public string[] t5 { get; set; }

        }

        #endregion

        public TestStructureNumeric LoadNumeric()
        {
            TestStructureNumeric n = new TestStructureNumeric();
            Random rnd = new Random();
            foreach (var item in n.GetType().GetProperties())
            {
                if (item.PropertyType == typeof(bool))
                    item.SetValue(n, (rnd.Next() % 2) == 1, null);
                else if (item.PropertyType == typeof(byte))
                    item.SetValue(n, ((byte)((byte)rnd.Next() % byte.MaxValue)), null);
                else if (item.PropertyType == typeof(short))
                    item.SetValue(n, ((short)(rnd.Next() % Int16.MaxValue)), null);
                else if (item.PropertyType == typeof(int))
                    item.SetValue(n, ((int)rnd.Next()), null);
                else if (item.PropertyType == typeof(long))
                    item.SetValue(n, ((long)rnd.Next()), null);
                else if (item.PropertyType == typeof(float))
                    item.SetValue(n, ((float)((float)rnd.NextDouble() % float.MaxValue)), null);
                else if (item.PropertyType == typeof(double))
                    item.SetValue(n, ((double)rnd.NextDouble()), null);
            }

            return n;

        }

        static string GetRandomString(Random rand)
        {
            string r = "";
            int len = rand.Next() % 255;

            for (int i = 0; i < len; i++)
            {
                r += (char)(rand.Next() % 64) + 5;
            }

            return r;
        }

        public TestStructureString LoadString(bool includenullinarray = false)
        {
            TestStructureString n = new TestStructureString();
            Random rnd = new Random();
            foreach (var item in n.GetType().GetProperties())
            {
                if (item.PropertyType == typeof(int))
                {
                    item.SetValue(n, (int)rnd.Next(1, 15), null);
                }
                else if (item.PropertyType == typeof(string[]))
                {
                    if (includenullinarray && rnd.Next() % 2 == 1)
                        continue;
                    int v1 = rnd.Next(10, 55);
                    var sl = new string[v1];
                    for (int h = 0; h < v1; h++)
                    {
                        sl[h] = GetRandomString(rnd);
                    }
                    item.SetValue(n, sl, null);
                }
                else if (item.PropertyType == typeof(string))
                {
                        item.SetValue(n, "", null);
                }
            }
            return n;
        }

        public TestStructureArray LoadArrays(bool includenullinarray = false)
        {
            TestStructureArray n = new TestStructureArray();
            Random rnd = new Random();
            n.len = 10;
            foreach (var item in n.GetType().GetProperties())
            {
                    if (item.PropertyType.IsArray)
                    {
                        if (includenullinarray && rnd.Next() % 2 == 1)
                            continue;
                    if (item.PropertyType == typeof(long[]))
                    {
                        long[] ar = new long[rnd.Next(0, 30)];
                        for (int i = 0; i < ar.Count(); i++)
                        {
                            ar[i] = rnd.Next();
                        }
                        item.SetValue(n, ar, null);
                    }
                    else if (item.PropertyType == typeof(float[]))
                    {
                        float[] ar = new float[rnd.Next(0, 30)];
                        for (int i = 0; i < ar.Count(); i++)
                        {
                            ar[i] = (float)(rnd.NextDouble() % float.MaxValue);
                        }
                        item.SetValue(n, ar, null);
                    }
                    else if (item.PropertyType == typeof(TestStructureNumeric[]))
                    {
                        int v1 = rnd.Next(0, 10);
                        TestStructureNumeric[] n1 = new TestStructureNumeric[v1];
                        for (int h = 0; h < v1; h++)
                        {
                            n1[h] = LoadNumeric();
                        }
                        item.SetValue(n, n1, null);
                    }
                }
                }
            return n;
        }
        public TestStructure LoadFULL(bool includenullinarray = false)
        {
            TestStructure TS = new TestStructure();
            Random rnd = new Random();
            foreach (var item in TS.GetType().GetProperties())
            {
                if (item.PropertyType.IsArray)
                {
                    if (includenullinarray && rnd.Next() % 2 == 1)
                        continue;
                    if (item.PropertyType == typeof(TestStructureString[]))
                    {
                        int v1 = rnd.Next(0, 10);
                        TestStructureString[] n1 = new TestStructureString[v1];
                        for (int h = 0; h < v1; h++)
                        {
                            n1[h] = LoadString(includenullinarray);
                        }
                        item.SetValue(TS, n1, null);
                    }
                    else if (item.PropertyType == typeof(TestStructureNumeric[]))
                    {
                        int v1 = rnd.Next(0, 10);
                        TestStructureNumeric[] n1 = new TestStructureNumeric[v1];
                        for (int h = 0; h < v1; h++)
                        {
                            n1[h] = LoadNumeric();
                        }
                        item.SetValue(TS, n1, null);
                    }
                    else if (item.PropertyType == typeof(TestStructureArray[]))
                    {
                        int v1 = rnd.Next(0, 10);
                        TestStructureArray[] n1 = new TestStructureArray[v1];
                        for (int h = 0; h < v1; h++)
                        {
                            n1[h] = LoadArrays(includenullinarray);
                        }
                        item.SetValue(TS, n1, null);
                    }

                }
                else
                {
                    if (item.PropertyType == typeof(TestStructureString))
                    {
                        TestStructureString n1 = LoadString(includenullinarray);
                        item.SetValue(TS, n1, null);
                    }
                    else if (item.PropertyType == typeof(TestStructureNumeric))
                    {
                        TestStructureNumeric n1 = LoadNumeric();
                        item.SetValue(TS, n1, null);
                    }
                    else if (item.PropertyType == typeof(TestStructureArray))
                    {
                        TestStructureArray n1 = LoadArrays(includenullinarray);
                        item.SetValue(TS, n1, null);
                    }
                }
            }
            return TS;
        }
    }
}
