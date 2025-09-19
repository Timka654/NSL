using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateConvertAttribute : Attribute
    {
        public FillTypeGenerateConvertAttribute(Type convertorType)
        {
            ConvertorType = convertorType;
        }

        public Type ConvertorType { get; }
    }
}
