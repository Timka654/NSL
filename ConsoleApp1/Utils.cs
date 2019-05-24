using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BinarySerializer_v5.Test
{
    class Utils
    {
        private static Random rand = new Random();

        private static char[] randStringTable = new char[]
            {
                'q','w','e','r','t','y','u','i','o','p','[',']','a','s','d','f','g','h','j','k','l',';','z','x','c','v','b','n','m',',','.','/','1','2','3','4','5','6','7','8','9','0','-','='
            };

        public static bool GetRandomBool()
        {
            return GetRandomI8() % 2 == 1;
        }

        public static byte GetRandomI8()
        {
            return ((byte)rand.Next(byte.MinValue, byte.MaxValue));
        }

        public static sbyte GetRandomSI8()
        {
            return ((sbyte)rand.Next(sbyte.MinValue, sbyte.MaxValue));
        }

        public static short GetRandomI16()
        {
            return ((short)rand.Next(short.MinValue, short.MaxValue));
        }

        public static ushort GetRandomUI16()
        {
            return ((ushort)rand.Next(ushort.MinValue, ushort.MaxValue));
        }

        public static int GetRandomI32()
        {
            return ((int)rand.Next(int.MinValue, int.MaxValue));
        }

        public static uint GetRandomUI32()
        {
            unchecked
            {
                return (uint)(rand.Next(0, int.MaxValue) * 2);
            }
        }


        public static int GetSize()
        {
            uint r = GetRandomUI32();

            return Convert.ToInt32(r % int.MaxValue) % 20;
        }

        public static long GetRandomI64()
        {
            return ((long)rand.Next(int.MinValue, int.MaxValue) * rand.Next(int.MinValue, int.MaxValue));
        }

        public static ulong GetRandomUI64()
        {
            unchecked
            {
                return (ulong)((rand.Next(0, int.MaxValue)) * rand.Next(1, 5));
            }
        }

        public static float GetRandomF32()
        {
            unchecked
            {
                return ((float)rand.NextDouble() % float.MaxValue);
            }
        }

        public static double GetRandomF64()
        {
            unchecked
            {
                return rand.NextDouble();
            }
        }

        public static DateTime GetRandomD()
        {
            int yyyy;
            int MM;
            int dd;
            int hh;
            int mm;
            int ss;
            int ms;
            return new DateTime(
                yyyy = 1970 + GetRandomI8(),
                MM = (GetRandomI8() % 11) + 1,
                dd = (GetRandomI8() % 28) + 1,
                hh = GetRandomI8() % 23,
                mm = GetRandomI8() % 40,
                ss = GetRandomI8() % 59,
                ms = GetRandomI8()
            );
        }

        public static Vector2 GetRandomV2()
        {
            return new Vector2(GetRandomI8(), GetRandomI8());
        }

        public static Vector3 GetRandomV3()
        {
            return new Vector3(GetRandomI8(), GetRandomI8(), GetRandomI8());
        }

        public static string GetRandomS()
        {
            int len = GetRandomI32() % 512;

            string r = "";

            for (int i = 0; i < len; i++)
            {
                r += randStringTable[rand.Next(0, randStringTable.Length)];
            }

            return r;
        }
    }
}
