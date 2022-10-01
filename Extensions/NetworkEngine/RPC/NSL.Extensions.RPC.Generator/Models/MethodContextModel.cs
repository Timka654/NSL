using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Declarations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Models
{
    internal class MethodContextModel
    {
        public MethodDeclModel Method { get; set; }

        public MethodDeclarationSyntax methodSyntax { get; set; }

        public SemanticModel SemanticModel { get; set; }

        public Compilation Compilation => Method.Class.Compilation;

    }
}
