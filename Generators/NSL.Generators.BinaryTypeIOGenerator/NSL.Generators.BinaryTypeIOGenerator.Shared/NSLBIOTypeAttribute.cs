using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class NSLBIOTypeAttribute : BinaryIODataAttribute
    {
        public NSLBIOTypeAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }
    }
}
