using System;

namespace NSL.Generators.SelectTypeCycleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SqlCycleSelectAttribute : Attribute
    {
        public string[] Models { get; }
        public SqlCycleSelectAttribute(params string[] models) => Models = models;

        /// <summary>
        /// When true, generates a standalone DTO class containing the members included for each model
        /// (those marked with <c>[SelectGenerateInclude]</c> and not overridden by a proxy).
        /// </summary>
        public bool Dto { get; set; }

        /// <summary>
        /// Suffix for the generated DTO class name to avoid conflicts when multiple generators produce DTOs
        /// for the same type and model. Default is "Cycle".
        /// Example: "FooModel" with model "Bar" → "FooDtoBarCycleModel".
        /// </summary>
        public string DtoSuffix { get; set; } = "Cycle";
    }
}
