using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateIncludeAttribute : Attribute
    {
        public FillTypeGenerateIncludeAttribute(params string[] models) { }
    }
}
