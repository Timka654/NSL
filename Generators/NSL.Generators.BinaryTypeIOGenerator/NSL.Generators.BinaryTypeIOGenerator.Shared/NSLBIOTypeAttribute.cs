using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class NSLBIOTypeAttribute : BinaryIODataAttribute
    {
        public NSLBIOTypeAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }

        /// <summary>
        /// When true, generates a standalone DTO class containing all members included for this model.
        /// </summary>
        public bool Dto { get; set; }

        /// <summary>
        /// Suffix appended to the generated DTO class name to avoid conflicts when multiple generators
        /// produce DTOs for the same type and model. Default is "BIO".
        /// Example: suffix "BIO" on "FooModel" with model "Bar" → "FooDtoBarBIOModel".
        /// </summary>
        public string DtoSuffix { get; set; } = "BIO";
    }
}
