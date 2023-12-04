using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateIgnoreAttribute : Attribute
    {
        public FillTypeGenerateIgnoreAttribute(Type fillTypeIgnore) { }
    }
}
