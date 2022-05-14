﻿using System;
using System.Security.Cryptography;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
namespace NSL.Cipher.RSA
{
    public class RSACipher : IPacketCipher
    {
        RSACryptoServiceProvider rsa;

        public RSACipher(string algName = "RSA", int keySize = 1024)
        {
            rsa = new RSACryptoServiceProvider();
            rsa.KeySize = keySize;
        }

        public void LoadXml(string xmlString)
        {
            rsa.FromXmlString(xmlString);
        }

        public string GetPublicKey()
        {
            return rsa.ToXmlString(false);
        }

        public string GetPrivateKey()
        {
            return rsa.ToXmlString(true);
        }

        public byte[] Decode(byte[] buffer, int offset, int lenght)
        {
            byte[] buf = new byte[lenght];
            Array.Copy(buffer,offset,buf,0,lenght);
            return rsa.Decrypt(buf,true);
        }

        public byte[] Encode(byte[] buffer, int offset, int lenght)
        {
            byte[] buf = new byte[lenght];
            Array.Copy(buffer, offset, buf, 0, lenght);
            return rsa.Encrypt(buf, true);
        }

        public byte[] Peek(byte[] buffer)
        {
            byte[] buf = new byte[InputPacketBuffer.headerLenght];
            Array.Copy(buffer, 0, buf, 0, InputPacketBuffer.headerLenght);
            return rsa.Encrypt(buf,true);
        }

        public IPacketCipher CreateEntry()
        {
            var r =  new RSACipher();
            r.LoadXml(GetPrivateKey());
            return r;
        }

        public bool Sync() => false;

        public void Dispose()
        {
            rsa?.Dispose();
        }
    }
}
