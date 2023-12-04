using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class SelectGenerateIncludeAttribute : Attribute
    {
        public SelectGenerateIncludeAttribute(params string[] models) { }
    }
}
