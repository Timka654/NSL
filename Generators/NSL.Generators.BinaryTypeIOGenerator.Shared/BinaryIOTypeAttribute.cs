using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [Obsolete("use NSLBIO attributes for use actual logic")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BinaryIOTypeAttribute : Attribute
    {
    }
}
