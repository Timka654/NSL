using System;

namespace NSL.Generators.FillTypeGenerator.Shared
{
    [Obsolete("Use FillTypeGenerateAttribute", true)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MergeToTypeAttribute : Attribute
    {
        public MergeToTypeAttribute(Type forType)
        {
            ForType = forType;
        }

        public Type ForType { get; }
    }
}
