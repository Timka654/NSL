using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Generators.SelectTypeGenerator
{
    public class SelectGenDTOContext : SelectGenContext
    {
        public override string GetTypeIdentifier(bool canNullable = true)
        {
            if (!Symbols.Any())
                return base.GetTypeIdentifier(canNullable);

            var className = Type.OriginalDefinition.ToString();

            if (className.EndsWith("Model"))
                className = className.Substring(0, className.Length - "Model".Length);

            className += $"Dto{Model}Model";

            return className;
        }

        public string GetTypeName()
        {
            var className = Type.Name;

            if (className.EndsWith("Model"))
                className = className.Substring(0, className.Length - "Model".Length);

            className += $"Dto{Model}Model";

            return className;
        }
    }
}
