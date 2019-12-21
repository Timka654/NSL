using System;
using System.Collections.Generic;
using System.Text;

namespace Cipher.SHA
{
    public class SHA1Cipher
    {
        public static string Hash(string input)
        {
            string r = "";
            using (System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] buf = Encoding.ASCII.GetBytes(input);
                buf = sha.ComputeHash(buf);

                for (int i = 0; i < buf.Length; i++)
                {
                    r += buf[i].ToString("x2");
                }
            }
            return r;
        }
    }
}
