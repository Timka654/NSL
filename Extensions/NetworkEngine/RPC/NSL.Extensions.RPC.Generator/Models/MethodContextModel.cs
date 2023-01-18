using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Declarations;

namespace NSL.Extensions.RPC.Generator.Models
{
    internal class MethodContextModel
    {
        public MethodDeclModel Method { get; set; }

        public MethodDeclarationSyntax methodSyntax { get; set; }

        public SemanticModel SemanticModel { get; set; }

        public Compilation Compilation => Method.Class.Compilation;

        public ParameterSyntax CurrentParameter { get; set; }

        public IParameterSymbol CurrentParameterSymbol { get; set; }

        public bool IsAsync { get; set; }

        public bool IsTask { get; set; }
    }
}
