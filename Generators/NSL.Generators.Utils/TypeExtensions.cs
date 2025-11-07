using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.Utils
{
    public static class TypeExtensions
    {
        public static bool HasPartialModifier(this TypeDeclarationSyntax type)
            => type.Modifiers.Any(x => x.ValueText.Equals("partial"));

        public static bool HasPublicModifier(this TypeDeclarationSyntax type)
            => type.Modifiers.Any(x => x.ValueText.Equals("public"));

        public static bool HasInternalModifier(this TypeDeclarationSyntax type)
            => type.Modifiers.Any(x => x.ValueText.Equals("internal"));

        public static string GetTypeClassName(this TypeDeclarationSyntax type)
            => type.Identifier.Text;

        public static string[] GetTypeClassUsingDirectives(this TypeDeclarationSyntax type)
            => (type.SyntaxTree.GetRoot() as CompilationUnitSyntax)?.Usings.Select(x => string.Join(" ", x.ToString().Split(' ').Skip(1)).TrimEnd(';')).ToArray();

        public static string[] GetTypeGenericParameters(this TypeDeclarationSyntax c)
            => c.TypeParameterList?.Parameters.Select(x => x.Identifier.Text).ToArray();

        public static string GetTypeFullName(this ITypeSymbol type, bool canNullable = true)
        {
            var t = type.ToString();

            if (!canNullable)
                t = t.TrimEnd('?');

            return t;
        }

        public static ISymbol[] GetAllMembers(this ITypeSymbol type, bool @interface = false)
        {
            var cType = type;

            List<ISymbol> toMembers = new List<ISymbol>();

            GetAllMembers(new[] { type }, toMembers, @interface);

            return toMembers.ToArray();
        }

        private static void GetAllMembers(this IEnumerable<ITypeSymbol> types, List<ISymbol> items, bool @interface = false)
        {
            foreach (var cType in types)
            {
                items.AddRange(cType.GetMembers());

                var a = (@interface ? cType.AllInterfaces.Cast<ITypeSymbol>() : new[] { cType.BaseType })
                    .Where(x => x != null);

                if (a.Any())
                    GetAllMembers(a, items, @interface);
            }
        }

    }
    public class TypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol>
    {
        public static TypeSymbolEqualityComparer Instance { get; } = new TypeSymbolEqualityComparer();
        public bool Equals(ITypeSymbol x, ITypeSymbol y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ITypeSymbol obj)
        {
            return obj.GetHashCode();
        }
    }
}
