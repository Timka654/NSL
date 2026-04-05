using System;

namespace NSL.Generators.FillTypeGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
    public class FillTypeFromGenerateAttribute : Attribute
    {
        /// <summary>
        /// Configure generate method for fill current object type to <paramref name="fillType"/> properties from object with current type
        /// </summary>
        /// <param name="fillType"></param>
        public FillTypeFromGenerateAttribute(Type fillType)
        {
            FillType = fillType;
        }

        /// <summary>
        /// Configure generate methods for fill current object type to <paramref name="fillType"/> properties from object with current type with fill model
        /// </summary>
        /// <param name="fillType"></param>
        /// <param name="models"></param>
        public FillTypeFromGenerateAttribute(Type fillType, params string[] models)
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
        /// </summary>
        public string DtoSuffix { get; set; } = "Fill";
    }
}
