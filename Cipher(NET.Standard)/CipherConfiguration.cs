using System.Security.Cryptography;

namespace Cipher
{
    /// <summary>
    /// Настройки криптографии
    /// </summary>
    public class CipherConfiguration
    {
        /// <summary>
        /// Тип криптографии
        /// </summary>
        public CipherMode CipherMode { get;set; }

        public PaddingMode CipherPadding { get;set; }

        /// <summary>
        /// Размер ключа
        /// </summary>
        public int KeySize { get;set; }

        /// <summary>
        /// Размер блока
        /// </summary>
        public int BlockSize { get;set; }
    }
}
