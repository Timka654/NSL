using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Shared
{
    [Obsolete("use NSLBIO attributes for use actual logic")]
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
