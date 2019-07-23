using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SCL.Cipher
{
    /// <summary>
    /// Настройки криптографии
    /// </summary>
    public class CipherConfiguration
    {
        /// <summary>
        /// Тип криптографии
        /// </summary>
        public CipherMode CipherMode { get; set; }

        /// <summary>
        /// Размер ключа
        /// </summary>
        public int KeySize { get; set; }

        /// <summary>
        /// Размер блока
        /// </summary>
        public int BlockSize { get; set; }
    }
}
