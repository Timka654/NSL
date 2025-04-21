using System.Collections.Generic;

namespace NSL.SMTP.ASPNET
{
    internal class SMTPByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj is null)
                return 0;

            // Fowler–Noll–Vo hash function (FNV-1a), good performance for small arrays
            unchecked
            {
                const int fnvPrime = 16777619;
                int hash = (int)2166136261;

                foreach (byte b in obj)
                {
                    hash ^= b;
                    hash *= fnvPrime;
                }

                return hash;
            }
        }
    }
}
