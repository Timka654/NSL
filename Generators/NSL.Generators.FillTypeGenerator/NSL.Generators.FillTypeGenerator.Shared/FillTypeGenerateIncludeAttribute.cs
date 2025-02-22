using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateIncludeAttribute : Attribute
    {
        /// <summary>
        /// Include property/field on generate method to fill with model
        /// </summary>
        /// <param name="models"></param>
        public FillTypeGenerateIncludeAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }
    }
}
