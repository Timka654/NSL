using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.MergeTypeGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Generators.MergeTypeGenerator
{
    internal class MergeTypeAttributeSyntaxReceiver : ISyntaxReceiver
    {
        public IList<TypeDeclarationSyntax> MergeToTypeTypes { get; } = new List<TypeDeclarationSyntax>();

        string MergeToTypeAttributeName = typeof(MergeToTypeAttribute).Name;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (typeDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => a.GetAttributeFullName().Equals(MergeToTypeAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        MergeToTypeTypes.Add(typeDeclarationSyntax);
                    }
                }
            }
        }
    }
}
