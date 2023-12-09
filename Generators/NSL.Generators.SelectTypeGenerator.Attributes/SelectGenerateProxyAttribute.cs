using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class SelectGenerateProxyAttribute : Attribute
    {
        public SelectGenerateProxyAttribute(string to) { }

        public SelectGenerateProxyAttribute(string from, string to) { }
    }
}
