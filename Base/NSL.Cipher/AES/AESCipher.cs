using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Security.Cryptography;

namespace NSL.Cipher.AES
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

        public AESCipher(Aes aes)
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

        public byte[] Decode(byte[] buffer, int offset, int length)
        {
            return decrypt.TransformFinalBlock(buffer, offset, length);
        }

        public byte[] Encode(byte[] buffer, int offset, int length)
        {
            return encrypt.TransformFinalBlock(buffer,offset,length);
        }

        public byte[] Peek(byte[] buffer)
        {
            return decrypt.TransformFinalBlock(buffer,0,InputPacketBuffer.DefaultHeaderLength);
        }

        public IPacketCipher CreateEntry()
        {
            return new AESCipher(aes);
        }

        public bool Sync() => false;

        public void Dispose()
        {
            aes?.Dispose();
        }
    }
}
