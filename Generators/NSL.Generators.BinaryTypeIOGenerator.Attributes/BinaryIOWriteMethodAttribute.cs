using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [Obsolete("This attribute already not supported, use BinaryIOMethodsForAttribute", true)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BinaryIOWriteMethodAttribute : Attribute
    {
        public string For { get; set; } = "*";
    }
}
