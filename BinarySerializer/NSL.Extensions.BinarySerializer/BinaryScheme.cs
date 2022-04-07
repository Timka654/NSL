using System;

namespace NSL.Extensions.BinarySerializer
{
    public class BinaryScheme
    {
        public Type Type { get; set; }

        public string Scheme { get; set; }

        public BinaryWriteAction WriteAction { get; set; }

        public BinaryReadAction ReadAction { get; set; }
    }
}
