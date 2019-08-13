using System;
using System.Collections.Generic;
using System.Text;

namespace ps.Data.NodeServer.Network
{
    public class Security
    {
        /// <summary>
        /// Данные шифрования AES криптографии
        /// </summary>
        public Cipher.AES.AESCipher AES;

        /// <summary>
        /// Данные шифрования RSA криптографии
        /// </summary>
        public Cipher.RSA.RSACipher RSA;
    }
}
