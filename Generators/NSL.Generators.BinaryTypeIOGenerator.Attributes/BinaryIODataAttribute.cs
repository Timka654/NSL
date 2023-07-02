using NSL.Generators.BinaryTypeIOGenerator.Attributes.Interface;
using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIODataAttribute : Attribute, IBinaryIOFor
    {
        public string For { get; set; } = "*";
    }
}
