using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NSL.Extensions.RPC.Generator
{

    internal class MethodDecl
    {
        public ClassDecl Class { get; set; }

        public string Name { get; set; }

        public IEnumerable<MethodDeclarationSyntax> Overrides { get; set; }
    }

    internal class ClassDecl
    {
        public ClassDeclarationSyntax Class { get; set; }

        public GeneratorExecutionContext Context { get; set; }

        public Compilation Compilation => Context.Compilation;

        public IEnumerable<MethodDecl> Methods { get; set; }
    }
}
