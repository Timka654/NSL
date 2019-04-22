using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public class Token
    {
        private string Value;

        public override string ToString()
        {
            return Value;
        }

        public static Token GenerateToken(int len = 128)
        {
            Token t = new Token();
            byte[] buffer = new byte[len / 2];
            rand.NextBytes(buffer);
            t.Value = "";

            for (int i = 0; i < buffer.Length; i++)
            {
                t.Value += buffer[i].ToString("x2");
            }
            return t;
        }

        private static Random rand = new Random();
    }
}
