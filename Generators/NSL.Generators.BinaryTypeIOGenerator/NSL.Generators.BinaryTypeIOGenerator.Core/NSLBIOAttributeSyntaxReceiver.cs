using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class NSLBIOAttributeSyntaxReceiver /* : ISyntaxReceiver*/
    {
        //public IList<TypeDeclarationSyntax> BinaryIOTypes { get; } = new List<TypeDeclarationSyntax>();

        static readonly string NSLBIOTypeAttributeName = typeof(NSLBIOTypeAttribute).Name;

        //public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        //{
        //    if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
        //    {
        //        if (typeDeclarationSyntax.AttributeLists.Count > 0)
        //        {
        //            if (typeDeclarationSyntax.AttributeLists
        //                .Any(al => al.Attributes
        //                    .Any(a => a.GetAttributeFullName().Equals(NSLBIOTypeAttributeName, StringComparison.InvariantCultureIgnoreCase))))
        //            {
        //                BinaryIOTypes.Add(typeDeclarationSyntax);
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
                            .Any(a => a.GetAttributeFullName().Equals(NSLBIOTypeAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
