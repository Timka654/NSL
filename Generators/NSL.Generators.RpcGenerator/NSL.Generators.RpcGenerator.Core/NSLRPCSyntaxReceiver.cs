using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.RpcGenerator.Shared.Attributes;
using NSL.Generators.Utils;
using System.Linq;
using System.Threading;

namespace NSL.Generators.RpcGenerator.Core
{
    internal class NSLRPCSyntaxReceiver
    {
        static readonly string AttributeName = typeof(NSLRPCImplementAttribute).Name;

        public static bool OnVisitSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (typeDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => a.GetAttributeFullName().Equals(AttributeName))))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
