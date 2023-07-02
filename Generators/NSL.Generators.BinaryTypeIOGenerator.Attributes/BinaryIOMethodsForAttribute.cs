using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class BinaryIOMethodsForAttribute : Attribute
    {
        public string[] For { get; set; }

        public BinaryIOMethodsForAttribute(params string[] @for)
        {
            For = @for;
        }
        public BinaryIOMethodsForAttribute()
        {
            For = new string[] { "*" };
        }
    }
}
