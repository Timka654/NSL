﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Extensions.RPC.Generator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Extensions.RPC.Generator
{
    internal class RPCMethodAttributeSyntaxReceiver : ISyntaxReceiver
    {
        public IList<MethodDeclarationSyntax> RPCMethods { get; } = new List<MethodDeclarationSyntax>();

        public IList<MethodDeclarationSyntax> RPCTypeHandleMethods { get; } = new List<MethodDeclarationSyntax>();

        string RPCMethodAttributeName = typeof(RPCMethodAttribute).Name;
        string RPCTypeHanleAttributeName = typeof(RPCTypeHandleAttribute).Name;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                if (methodDeclarationSyntax.AttributeLists.Count > 0)
                {
                    if (methodDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => FixAttributeName(a.Name.ToString()).Equals(RPCMethodAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        RPCMethods.Add(methodDeclarationSyntax);
                    }
                    else if (methodDeclarationSyntax.AttributeLists
                        .Any(al => al.Attributes
                            .Any(a => FixAttributeName(a.Name.ToString()).Equals(RPCTypeHanleAttributeName, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        RPCTypeHandleMethods.Add(methodDeclarationSyntax);
                    }
                }
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
