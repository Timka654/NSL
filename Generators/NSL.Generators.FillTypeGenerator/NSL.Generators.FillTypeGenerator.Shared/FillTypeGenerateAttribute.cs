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
        public FillTypeGenerateAttribute(Type fillType)
        {
            FillType = fillType;
        }

        /// <summary>
        /// Configure generate methods for fill current object type to <paramref name="fillType"/> properties from object with current type with fill model
        /// </summary>
        /// <param name="fillType"></param>
        /// <param name="models"></param>
        public FillTypeGenerateAttribute(Type fillType, params string[] models)
        {
            FillType = fillType;
            Models = models;
        }

        public Type FillType { get; }
        public string[] Models { get; }

        /// <summary>
        /// When true, generates a standalone DTO class containing all members included for each configured model.
        /// </summary>
        public bool Dto { get; set; }

        /// <summary>
        /// Suffix for the generated DTO class name to avoid conflicts when multiple generators produce DTOs
        /// for the same type and model. Default is "Fill".
        /// Example: "FooModel" with model "Bar" → "FooDtoBarFillModel".
        /// </summary>
        public string DtoSuffix { get; set; } = "Fill";
    }
}
