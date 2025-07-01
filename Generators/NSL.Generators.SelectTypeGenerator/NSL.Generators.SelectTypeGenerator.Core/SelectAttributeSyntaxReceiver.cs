using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.SelectTypeGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Linq;
using System.Threading;

namespace NSL.Generators.SelectTypeGenerator
{
    internal class SelectAttributeSyntaxReceiver /*: ISyntaxReceiver*/
    {
        //public IList<TypeDeclarationSyntax> SelectTypes { get; } = new List<TypeDeclarationSyntax>();

        static readonly string SelectAttributeName = typeof(SelectGenerateAttribute).Name;

        //public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        //{
        //    if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
        //    {
        //        if (typeDeclarationSyntax.AttributeLists.Count > 0)
        //        {
        //            if (typeDeclarationSyntax.AttributeLists
        //                .Any(al => al.Attributes
        //                    .Any(a => a.GetAttributeFullName().Equals(SelectAttributeName, StringComparison.InvariantCultureIgnoreCase))))
        //            {
        //                SelectTypes.Add(typeDeclarationSyntax);
        //            }
        //        }
        //    }
        //}

        public static bool OnVisitSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (typeDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => a.GetAttributeFullName().Equals(SelectAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
