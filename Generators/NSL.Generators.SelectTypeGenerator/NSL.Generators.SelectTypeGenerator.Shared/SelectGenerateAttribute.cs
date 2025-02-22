using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SelectGenerateAttribute : Attribute
    {
        /// <summary>
        /// Configure generate extensions "Select<model>" methods for <see cref="System.Collections.Generic.IEnumerable{T}"/> and <see cref="System.Linq.IQueryable{T}"/> for models
        /// </summary>
        /// <param name="models"></param>
        public SelectGenerateAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }
    }
}
