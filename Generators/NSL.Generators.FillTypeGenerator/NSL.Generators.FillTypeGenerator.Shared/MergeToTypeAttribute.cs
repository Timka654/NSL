﻿using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
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
