﻿using System;
using System.Security.Cryptography;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Cipher.DES
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

        public bool DecodeRef(ref byte[] buffer, int offset, int length) => throw new NotImplementedException();

        public bool DecodeHeaderRef(ref byte[] buffer, int offset) => throw new NotImplementedException();

        public bool EncodeRef(ref byte[] buffer, int offset, int length) => throw new NotImplementedException();

        public bool EncodeHeaderRef(ref byte[] buffer, int offset) => throw new NotImplementedException();

        public byte[] Decode(byte[] buffer, int offset, int length)
        {
            return decrypt.TransformFinalBlock(buffer, offset, length);
        }

        public byte[] Encode(byte[] buffer, int offset, int length)
        {
            return encrypt.TransformFinalBlock(buffer, offset, length);
        }

        public byte[] Peek(byte[] buffer)
        {
            return decrypt.TransformFinalBlock(buffer, 0, InputPacketBuffer.DefaultHeaderLength);
        }

        public void Peek(ArraySegment<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public IPacketCipher CreateEntry()
        {
            return new DESCipher(des);
        }

        public bool Sync() => false;

        public void Dispose()
        {
            des?.Dispose();
        }
    }
}
