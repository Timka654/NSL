using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Comparers
{
    internal class MethodDeclarationSyntaxComparer : EqualityComparer<MethodDeclarationSyntax>
    {
        public override bool Equals(MethodDeclarationSyntax x, MethodDeclarationSyntax y)
        {
            return x.Identifier.Text.Equals(y.Identifier.Text);
        }

        public override int GetHashCode(MethodDeclarationSyntax obj)
        {
            return obj.GetHashCode();
        }
    }
}
