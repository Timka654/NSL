using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateIgnoreAttribute : Attribute
    {
        public Type FillTypeIgnore { get; }

        /// <summary>
        /// Mark as ignore property/field on fill to/from type <paramref name="fillTypeIgnore"/>
        /// </summary>
        /// <param name="fillTypeIgnore"></param>
        public FillTypeGenerateIgnoreAttribute(Type fillTypeIgnore)
        {
            FillTypeIgnore = fillTypeIgnore;
        }
        /// <summary>
        /// Mark as ignore property/field on fill any
        /// </summary>
        public FillTypeGenerateIgnoreAttribute() { }
    }
}
