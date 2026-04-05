using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Shared
{
    [Obsolete("use NSLBIO attributes for use actual logic")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BinaryIOTypeAttribute : Attribute
    {
    }
}
