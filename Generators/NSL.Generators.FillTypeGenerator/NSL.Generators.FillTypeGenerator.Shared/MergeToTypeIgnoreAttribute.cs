using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [Obsolete("Use FillTypeGenerateIgnoreAttribute", true)]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MergeToTypeIgnoreAttribute : Attribute
    {
        public MergeToTypeIgnoreAttribute(Type forTypeIgnore)
        {
            ForTypeIgnore = forTypeIgnore;
        }

        public Type ForTypeIgnore { get; }
    }
}
