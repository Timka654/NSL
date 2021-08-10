using System.Security.Cryptography;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Cipher.AES
{
    public class AESCipher : IPacketCipher
    {
        Aes aes = Aes.Create();

        ICryptoTransform encrypt;
        ICryptoTransform decrypt;

        public AESCipher(CipherConfiguration configuration)
        { 
            aes.Mode = configuration.CipherMode;
            aes.KeySize = configuration.KeySize;
            aes.BlockSize = configuration.BlockSize;
            aes.Padding = configuration.CipherPadding;
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
