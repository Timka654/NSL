using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BinaryIOReadMethodAttribute : Attribute
    {
        public string For { get; set; } = "*";
    }
}
