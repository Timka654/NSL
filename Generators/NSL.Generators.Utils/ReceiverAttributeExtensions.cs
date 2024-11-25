using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

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
        public static string GetClassFullModifier(this ClassDeclarationSyntax classDecl, IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            var mods = new List<string>(classDecl.GetClassModifiers());

            if (exclude != null)
                foreach (var item in exclude)
                {
                    mods.Remove(item);
                }

            if (include != null)
                foreach (var item in include)
                {
                    if (!mods.Contains(item))
                        mods.Add(item);
                }

            return string.Join(" ", mods);
        }
        public static IEnumerable<string> GetClassModifiers(this ClassDeclarationSyntax classDecl)
            => classDecl.Modifiers.Select(x => x.Text);

        public static string GetClassName(this ClassDeclarationSyntax classDecl)
            => @classDecl.Identifier.Text;

        public static string GetMethodName(this MethodDeclarationSyntax methodDecl)
            => methodDecl.Identifier.Text;

    }
}
