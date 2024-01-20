using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class SelectGenerateIncludeAttribute : Attribute
    {
        /// <summary>
        /// Include property to model for select generate
        /// </summary>
        /// <param name="models"></param>
        public SelectGenerateIncludeAttribute(params string[] models) { }
    }
}
