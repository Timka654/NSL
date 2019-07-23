using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using SCL.SocketClient.Utils.Buffer;
using SCL.SocketClient.Utils;

namespace SCL.Cipher.AES
{
    public class AESCipher : IPacketCipher
    {
        Aes aes = Aes.Create();

        ICryptoTransform encrypt;
        ICryptoTransform decrypt;

        public static AESCipher FromKey(byte[] cipher_key, CipherConfiguration configuration)
        {
            var aes = new AESCipher(configuration);

            byte[] key = new byte[cipher_key.Length / 2];
            Array.Copy(cipher_key, key, key.Length);
            aes.aes.Key = key;

            byte[] iv = new byte[cipher_key.Length / 2];
            Array.Copy(cipher_key, iv, iv.Length);
            aes.aes.Key = iv;

            aes.encrypt = aes.aes.CreateEncryptor();
            aes.decrypt = aes.aes.CreateDecryptor();

            return aes;
        }

        public AESCipher(CipherConfiguration configuration)
        { 
            aes.Mode = configuration.CipherMode;
            aes.KeySize = configuration.KeySize;
            aes.BlockSize = configuration.BlockSize;
        }

        private AESCipher(Aes aes)
        {
            this.aes.Mode = aes.Mode;
            this.aes.KeySize = aes.KeySize;
            this.aes.BlockSize = aes.BlockSize;
            this.aes.Key = aes.Key;
            this.aes.IV = aes.IV;

            encrypt = aes.CreateEncryptor();
            decrypt = aes.CreateDecryptor();
        }

        public void GenerateKeys()
        {
            aes.GenerateKey();
            if(aes.Mode == CipherMode.CBC)
                aes.GenerateIV();

            encrypt = aes.CreateEncryptor();
            decrypt = aes.CreateDecryptor();
        }

        public byte[] GetKey()
        {
            return aes.Key;
        }

        public byte[] GetIV()
        {
            return aes.IV;
        }

        public byte[] Decode(byte[] buffer, int offset, int lenght)
        {
            return decrypt.TransformFinalBlock(buffer, offset, lenght);
        }

        public byte[] Encode(byte[] buffer, int offset, int lenght)
        {
            return encrypt.TransformFinalBlock(buffer,offset,lenght);
        }

        public byte[] Peek(byte[] buffer)
        {
            return decrypt.TransformFinalBlock(buffer,0,InputPacketBuffer.headerLenght);
        }

        public object Clone()
        {
            return new AESCipher(aes);
        }
    }
}
