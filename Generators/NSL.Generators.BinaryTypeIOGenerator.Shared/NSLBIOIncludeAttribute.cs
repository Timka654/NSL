using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NSLBIOIncludeAttribute : Attribute
    {
        public NSLBIOIncludeAttribute(params string[] models) { }
    }
}
