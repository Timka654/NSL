using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FillTypeGenerateAttribute : Attribute
    {
        /// <summary>
        /// Configure generate method for fill current object type to <paramref name="fillType"/> properties from object with current type
        /// </summary>
        /// <param name="fillType"></param>
        public FillTypeGenerateAttribute(Type fillType) { }

        /// <summary>
        /// Configure generate methods for fill current object type to <paramref name="fillType"/> properties from object with current type with fill model
        /// </summary>
        /// <param name="fillType"></param>
        /// <param name="models"></param>
        public FillTypeGenerateAttribute(Type fillType, params string[] models) { }
    }
}
