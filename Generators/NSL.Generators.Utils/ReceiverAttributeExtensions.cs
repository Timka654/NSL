using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Generators.Utils
{
    public static class ReceiverAttributeExtensions
    {
        public static string GetAttributeFullName(this AttributeSyntax attr)
        {
            var name = attr.Name.ToString();

            if (!name.EndsWith("Attribute"))
                return $"{name}Attribute";

            return name;
        }

        public static string GetClassFullModifier(this ClassDeclarationSyntax classDecl)
            => string.Join(" ", classDecl.GetClassModifiers());

        public static IEnumerable<string> GetClassModifiers(this ClassDeclarationSyntax classDecl)
            => classDecl.Modifiers.Select(x => x.Text);

        public static string GetClassName(this ClassDeclarationSyntax classDecl)
            => @classDecl.Identifier.Text;

        public static string GetMethodName(this MethodDeclarationSyntax methodDecl)
            => methodDecl.Identifier.Text;

    }
}
