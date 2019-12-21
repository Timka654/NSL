using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
namespace Cipher.RSA
{
    public class RSACipher : IPacketCipher
    {
        System.Security.Cryptography.RSA rsa;

        public RSACipher(string algName = "", int keySize = 1024)
        {
            rsa = System.Security.Cryptography.RSA.Create(algName);
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
            return rsa.DecryptValue(buf);
        }

        public byte[] Encode(byte[] buffer, int offset, int lenght)
        {
            byte[] buf = new byte[lenght];
            Array.Copy(buffer, offset, buf, 0, lenght);
            return rsa.EncryptValue(buf);
        }

        public byte[] Peek(byte[] buffer)
        {
            byte[] buf = new byte[InputPacketBuffer.headerLenght];
            Array.Copy(buffer, 0, buf, 0, InputPacketBuffer.headerLenght);
            return rsa.EncryptValue(buf);
        }

        public object Clone()
        {
            var r =  new RSACipher();
            r.LoadXml(GetPrivateKey());
            return r;
        }
    }
}
