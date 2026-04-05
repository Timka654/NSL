using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NSLBIOIncludeAttribute : Attribute
    {
        public NSLBIOIncludeAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }
    }
}
