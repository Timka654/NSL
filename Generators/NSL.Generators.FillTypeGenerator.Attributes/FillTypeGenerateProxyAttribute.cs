using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateProxyAttribute : Attribute
    {
        public FillTypeGenerateProxyAttribute(string to) { }

        public FillTypeGenerateProxyAttribute(string from, string to) { }
    }
}
