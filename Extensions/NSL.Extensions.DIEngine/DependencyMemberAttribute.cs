﻿using System;

namespace NSL.Extensions.DIEngine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DependencyMemberAttribute : Attribute
    {
        public Type Type { get; private set; }

        public DependencyMemberAttribute() { }

        public DependencyMemberAttribute(Type t) { Type = t; }
    }
}