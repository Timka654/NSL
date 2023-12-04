using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [Obsolete("Use FillTypeGenerateIgnoreAttribute", true)]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MergeToTypeIgnoreAttribute : Attribute
    {
        public MergeToTypeIgnoreAttribute(Type forTypeIgnore) { }
    }
}
