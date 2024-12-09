using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.PacketHandleGenerator
{
    internal class NSLPHAttributeSyntaxReceiver : ISyntaxReceiver
    {
        public IList<TypeDeclarationSyntax> BinaryIOTypes { get; } = new List<TypeDeclarationSyntax>();

        string NSLBIOTypeAttributeName = typeof(NSLPHGenImplementAttribute).Name;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (typeDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => a.GetAttributeFullName().Equals(NSLBIOTypeAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        BinaryIOTypes.Add(typeDeclarationSyntax);
                    }
                }
            }
        }
    }
}
