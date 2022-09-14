using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Extensions.RPC.Generator
{
    internal class RPCMethodAttributeSyntaxReceiver<TAttribute> : ISyntaxReceiver
        where TAttribute : Attribute
    {
        public IList<MethodDeclarationSyntax> Methods { get; } = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax &&
                methodDeclarationSyntax.AttributeLists.Count > 0 &&
                methodDeclarationSyntax.AttributeLists
                    .Any(al => al.Attributes
                        .Any(a => FixAttributeName(a.Name.ToString()).Equals(typeof(TAttribute).Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                Methods.Add(methodDeclarationSyntax);
            }
        }

        private static string FixAttributeName(string name)
        {
            if (!name.EndsWith("Attribute"))
                return $"{name}Attribute";

            return name;
        }
    }
}
