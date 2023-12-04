using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.FillTypeGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Generators.FillTypeGenerator
{
    internal class FillTypeAttributeSyntaxReceiver : ISyntaxReceiver
    {
        public IList<TypeDeclarationSyntax> FillTypeTypes { get; } = new List<TypeDeclarationSyntax>();

        string FillTypeAttributeName = typeof(FillTypeGenerateAttribute).Name;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (typeDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => a.GetAttributeFullName().Equals(FillTypeAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        FillTypeTypes.Add(typeDeclarationSyntax);
                    }
                }
            }
        }
    }
}
