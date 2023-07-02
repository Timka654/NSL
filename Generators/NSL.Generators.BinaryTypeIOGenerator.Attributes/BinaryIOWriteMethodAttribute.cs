using NSL.Generators.BinaryTypeIOGenerator.Attributes.Interface;
using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BinaryIOWriteMethodAttribute : Attribute, IBinaryIOFor
    {
        public string For { get; set; } = "*";
    }
}
