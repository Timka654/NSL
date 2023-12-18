using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static string[] GetTypeGenericParameters(this ClassDeclarationSyntax c)
            => c.TypeParameterList?.Parameters.Select(x => x.Identifier.Text).ToArray();

        public static string GetTypeFullName(this ITypeSymbol type)
            => type.ToString();

        public static ISymbol[] GetAllMembers(this ITypeSymbol type)
        {
            var cType = type;

            List<ISymbol> toMembers = new List<ISymbol>();

            do
            {
                toMembers.AddRange(cType.GetMembers());

                cType = cType.BaseType;
            } while (cType != null);

            return toMembers.ToArray();
        }

    }
}
