using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NSL.Generators.HttpEndPointGenerator
{
    internal class HttpEndPointImplementAttributeSyntaxReceiver/* : ISyntaxReceiver*/
    {
        //public IList<TypeDeclarationSyntax> Types { get; } = new List<TypeDeclarationSyntax>();

        static readonly string AttributeName = typeof(HttpEndPointImplementGenerateAttribute).Name;

        //public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        //{
        //    if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
        //    {
        //        if (typeDeclarationSyntax.AttributeLists.Count > 0)
        //        {
        //            if (typeDeclarationSyntax.AttributeLists
        //                .Any(al => al.Attributes
        //                    .Any(a => a.GetAttributeFullName().Equals(AttributeName, StringComparison.InvariantCultureIgnoreCase))))
        //            {
        //                Types.Add(typeDeclarationSyntax);
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
                            .Any(a => a.GetAttributeFullName().Equals(AttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
