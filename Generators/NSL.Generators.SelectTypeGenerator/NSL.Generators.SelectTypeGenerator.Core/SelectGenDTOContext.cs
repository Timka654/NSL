using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using System.Linq;

namespace NSL.Generators.SelectTypeGenerator.Core
{
    public class SelectGenDTOContext : SelectGenContext
    {
        /// <summary>
        /// Suffix appended to the generated DTO class name to prevent conflicts when multiple generators
        /// produce DTOs for the same type+model. Default is empty (SelectTypeGenerator convention).
        /// </summary>
        public string DtoSuffix { get; set; } = "";

        public override string GetTypeIdentifier(bool canNullable = true)
        {
            if (!Symbols.Any())
                return base.GetTypeIdentifier(canNullable);

            return BuildDtoFullName();
        }

        private string BuildDtoFullName()
        {
            var fullName = Type.GetTypeFullName(false);

            if (fullName.EndsWith("Model"))
                fullName = fullName.Substring(0, fullName.Length - "Model".Length);

            return $"{fullName}Dto{Model}{DtoSuffix}Model";
        }

        public string GetTypeName()
        {
            var className = Type.Name;

            if (className.EndsWith("Model"))
                className = className.Substring(0, className.Length - "Model".Length);

            return $"{className}Dto{Model}{DtoSuffix}Model";
        }
    }
}
