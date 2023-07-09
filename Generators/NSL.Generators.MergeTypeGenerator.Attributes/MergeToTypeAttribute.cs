using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.MergeTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MergeToTypeAttribute : Attribute
    {
        public MergeToTypeAttribute(Type forType) { }
    }
}
