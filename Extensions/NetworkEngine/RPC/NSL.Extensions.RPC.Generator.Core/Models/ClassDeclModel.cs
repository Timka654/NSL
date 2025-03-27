using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NSL.Extensions.RPC.Generator.Declarations
{
    internal class ClassDeclModel
    {
        public ClassDeclarationSyntax Class { get; set; }

        public GeneratorSyntaxContext Context { get; set; }

        public SemanticModel SemanticModel => Context.SemanticModel;

        public Compilation Compilation => SemanticModel.Compilation;

        public IEnumerable<MethodDeclModel> Methods { get; set; }

        public INamedTypeSymbol ClassSymbol { get; set; }
    }
}
