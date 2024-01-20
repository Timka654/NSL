﻿using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Mark as ignore property/field on fill to type <paramref name="fillTypeIgnore"/>
        /// </summary>
        /// <param name="fillTypeIgnore"></param>
        public FillTypeGenerateIgnoreAttribute(Type fillTypeIgnore) { }
    }
}
