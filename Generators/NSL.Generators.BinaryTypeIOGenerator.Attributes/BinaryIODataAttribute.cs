using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIODataAttribute : Attribute
    {
        public BinaryIODataAttribute(params string[] @for)
        {

        }
    }
}
