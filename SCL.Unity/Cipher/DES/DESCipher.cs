using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SCL.SocketClient.Utils;
using SCL.SocketClient.Utils.Buffer;

namespace SCL.Cipher.DES
{
    public class DESCipher : IPacketCipher
    {
        System.Security.Cryptography.DES des = System.Security.Cryptography.DES.Create();

        ICryptoTransform encrypt;
        ICryptoTransform decrypt;

        public DESCipher(CipherConfiguration configuration)
        {
            des.Mode = configuration.CipherMode;
            des.KeySize = configuration.KeySize;
            des.BlockSize = configuration.BlockSize;
        }

        private DESCipher(System.Security.Cryptography.DES des)
        {
            this.des.Mode = des.Mode;
            this.des.KeySize = des.KeySize;
            this.des.BlockSize = des.BlockSize;

            this.des.Key = des.Key;
            this.des.IV = des.IV;

            encrypt = des.CreateEncryptor();
            decrypt = des.CreateDecryptor();
        }

        public void GenerateKeys()
        {
            des.GenerateKey();
            if (des.Mode == CipherMode.CBC)
                des.GenerateIV();

            encrypt = des.CreateEncryptor();
            decrypt = des.CreateDecryptor();
        }

        public byte[] GetKey()
        {
            return des.Key;
        }

        public byte[] GetIV()
        {
            return des.IV;
        }

        public byte[] Decode(byte[] buffer, int offset, int lenght)
        {
            return decrypt.TransformFinalBlock(buffer, offset, lenght);
        }

        public byte[] Encode(byte[] buffer, int offset, int lenght)
        {
            return encrypt.TransformFinalBlock(buffer, offset, lenght);
        }

        public byte[] Peek(byte[] buffer)
        {
            return decrypt.TransformFinalBlock(buffer, 0, InputPacketBuffer.headerLenght);
        }

        public object Clone()
        {
            return new DESCipher(des);
        }
    }
}
