using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Generators.Utils
{
    public static class SummaryExtensions
    {
        public static string GetMethodSeeCRef(this IMethodSymbol method)
            => $"{method.ContainingType.Name}.{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Type.ToString().Replace('<', '{').Replace('>', '}')))})";

        public static string GetTypeSeeCRef(this ITypeSymbol type)
            => $"{type.GetTypeFullName()}";
    }
}
