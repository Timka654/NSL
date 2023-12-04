using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FillTypeGenerateAttribute : Attribute
    {
        public FillTypeGenerateAttribute(Type fillType) { }

        public FillTypeGenerateAttribute(Type fillType, params string[] models) { }
    }
}
