using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
#if !SCL
using SocketServer.Utils;
using SocketServer.Utils.Buffer;
#else
using SCL.SocketClient.Utils;
using SCL.SocketClient.Utils.Buffer;
#endif

namespace Cipher.MD5
{
    public class MD5Cipher
    {
        public static string Hash(string input)
        {
            string r = "";
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] buf = Encoding.ASCII.GetBytes(input);
                buf = md5.ComputeHash(buf);

                for (int i = 0; i < buf.Length; i++)
                {
                    r += buf[i].ToString("x2");
                }
            }
            return r;
        }
    }
}
