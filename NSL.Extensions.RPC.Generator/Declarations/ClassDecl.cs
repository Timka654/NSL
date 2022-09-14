using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Declarations
{

    internal class ClassDecl
    {
        public ClassDeclarationSyntax Class { get; set; }

        public GeneratorExecutionContext Context { get; set; }

        public Compilation Compilation => Context.Compilation;

        public IEnumerable<MethodDecl> Methods { get; set; }

        public INamedTypeSymbol ClassSymbol { get; set; }
    }
}
