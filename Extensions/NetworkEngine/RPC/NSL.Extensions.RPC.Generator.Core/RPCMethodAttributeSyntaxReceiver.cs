using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Extensions.RPC.Generator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NSL.Extensions.RPC.Generator
{
    internal class RPCMethodAttributeSyntaxReceiver
    {
        //public IList<MethodDeclarationSyntax> RPCMethods { get; } = new List<MethodDeclarationSyntax>();

        //public IList<MethodDeclarationSyntax> RPCTypeHandleMethods { get; } = new List<MethodDeclarationSyntax>();

        static readonly string RPCMethodAttributeName = typeof(RPCMethodAttribute).Name;
        static readonly string RPCTypeHandleAttributeName = typeof(RPCTypeHandleAttribute).Name;

        public static bool OnVisitSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                if (methodDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (methodDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => a.GetAttributeFullName().Equals(RPCMethodAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        return true;
                    }
                    //else if (methodDeclarationSyntax.AttributeLists
                    //    .Any(al => al.Attributes
                    //        .Any(a => a.GetAttributeFullName().Equals(RPCTypeHandleAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    //{
                    //    RPCTypeHandleMethods.Add(methodDeclarationSyntax);
                    //}
                }
            }

            return false;
        }
    }
}
