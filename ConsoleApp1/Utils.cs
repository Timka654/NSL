using System;
using System.Collections.Generic;
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
