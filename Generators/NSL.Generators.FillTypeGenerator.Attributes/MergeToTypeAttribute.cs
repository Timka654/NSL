using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [Obsolete("Use FillTypeGenerateAttribute", true)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MergeToTypeAttribute : Attribute
    {
        public MergeToTypeAttribute(Type forType) { }
    }
}
