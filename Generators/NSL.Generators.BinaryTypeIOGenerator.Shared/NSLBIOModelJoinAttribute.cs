using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class NSLBIOModelJoinAttribute : Attribute
    {
        public NSLBIOModelJoinAttribute(string model, params string[] joinModels) { }
    }
}
