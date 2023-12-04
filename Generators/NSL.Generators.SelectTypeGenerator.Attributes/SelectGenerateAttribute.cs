using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SelectGenerateAttribute : Attribute
    {
        public SelectGenerateAttribute(params string[] models) { }
    }
}
