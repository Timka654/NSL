using NSL.Generators.BinaryTypeIOGenerator.Attributes.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class BinaryIODataAttribute : Attribute, IBinaryIOFor
    {
        public string For { get; set; } = "*";
    }
}
