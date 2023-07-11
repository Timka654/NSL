using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BinaryIOWriteMethodAttribute : Attribute
    {
        public string For { get; set; } = "*";
    }
}
