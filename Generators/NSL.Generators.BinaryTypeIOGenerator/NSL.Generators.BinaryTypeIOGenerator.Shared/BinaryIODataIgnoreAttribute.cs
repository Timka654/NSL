using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Shared
{
    [Obsolete("use NSLBIO attributes for use actual logic")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIODataIgnoreAttribute : Attribute
    {
        public BinaryIODataIgnoreAttribute(params string[] @for)
        {
            For = @for;
        }

        public string[] For { get; }
    }
}
