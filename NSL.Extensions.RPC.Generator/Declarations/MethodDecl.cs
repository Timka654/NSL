using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Declarations
{
    internal class MethodDecl
    {
        public ClassDecl Class { get; set; }

        public string Name { get; set; }

        public IEnumerable<MethodDeclarationSyntax> Overrides { get; set; }
    }
}
