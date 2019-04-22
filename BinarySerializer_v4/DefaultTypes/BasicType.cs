using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer.DefaultTypes
{
    internal enum PropertyTypeEnum
    {
        FixedBasicType,
        DynamicBasicType,
        FixedBasicArrayType,
        DynamicBasicArrayType,
        FixedBasicListType,
        DynamicBasicListType,
        ClassType,
        ClassArrayType,
        ClassListType
    }

    public abstract class BasicType
    {
        protected BinarySerializer Serializer { get; private set; }

        public abstract bool FixedSize { get; }

        public abstract int Size { get; set; }

        public abstract void GetBytes(ref byte[] buffer, int offset, object value);

        public abstract object GetValue(ref byte[] buffer, int offset);

        public static int GetSize(object value)
        {
            return 0;
        }

        public void SetSerializer(BinarySerializer bs)
        {
            Serializer = bs;
        }
    }
}
